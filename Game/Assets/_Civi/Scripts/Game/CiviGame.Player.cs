using UnityEngine;
using UnityEngine.InputSystem;

namespace Civi
{
    public sealed partial class CiviGame
    {
        const int PlayerLayer = 2; // Ignore Raycast: the camera must not treat Mai's own capsule as a wall
        const float WalkSpeed = 3f, RunSpeed = 5.6f, Gravity = 16, JumpSpeed = 5.4f;

        Transform player;
        CiviBody playerBody;
        CharacterController controller;
        Camera view;
        float camYaw = 200, camPitch = 12, camDistance = 3.6f, smoothDistance = 3.6f, verticalSpeed, titleClock, cinematicBlend;
        Vector3 moveVelocity, camSmoothPos;
        Quaternion camSmoothRot = Quaternion.identity;

        void BootPlayer()
        {
            view = Camera.main;
            view.farClipPlane = 2600;
            player = bodies["mai"].transform;
            playerBody = bodies["mai"];
            player.gameObject.SetActive(true);
            player.gameObject.layer = PlayerLayer;
            controller = player.gameObject.AddComponent<CharacterController>();
            controller.center = new Vector3(0, .84f, 0);
            controller.height = 1.6f;
            controller.radius = .26f;
            controller.skinWidth = .03f;
            controller.stepOffset = .3f;
            controller.slopeLimit = 50;
            controller.minMoveDistance = 0; // the default swallows small steps on very fast machines
            playerBody.footfall = () => hub.Foot();
            camSmoothPos = view.transform.position;
            camSmoothRot = view.transform.rotation;
        }

        void TeleportPlayer(Vector3 pos, float yaw)
        {
            controller.enabled = false;
            player.position = pos;
            player.rotation = Quaternion.Euler(0, yaw, 0);
            controller.enabled = true;
            camYaw = yaw;
            camPitch = 12;
            moveVelocity = Vector3.zero;
            verticalSpeed = 0;
            drone.position = pos + Vector3.up * 1.7f;
            SnapCamera();
        }

        void SnapCamera() { camSmoothPos = Vector3.zero; cinematicBlend = -1; }

        void Idle()
        {
            playerBody.moveSpeed = Mathf.MoveTowards(playerBody.moveSpeed, 0, Time.deltaTime * 14);
            moveVelocity = Vector3.zero;
            playerBody.airborne = false;
            if (controller.enabled && player.gameObject.activeInHierarchy) controller.Move(Vector3.down * Time.deltaTime * 2);
        }

        void MovePlayer(Vector2 input, bool run, bool jump, float dt)
        {
            dt = Mathf.Min(dt, .05f);
            bool grounded = controller.isGrounded;
            if (grounded && verticalSpeed < 0) verticalSpeed = -2;
            if (grounded && jump) { verticalSpeed = JumpSpeed; grounded = false; }
            verticalSpeed -= Gravity * dt;
            var yaw = Quaternion.Euler(0, camYaw, 0);
            Vector3 wish = yaw * new Vector3(input.x, 0, input.y);
            float top = run ? RunSpeed : WalkSpeed;
            moveVelocity = Vector3.MoveTowards(moveVelocity, wish * top, (input.sqrMagnitude > 0 ? 24 : 34) * dt);
            var motion = moveVelocity * dt + Vector3.up * verticalSpeed * dt;
            controller.Move(motion);
            if (wish.sqrMagnitude > .01f)
                player.rotation = Quaternion.RotateTowards(player.rotation, Quaternion.LookRotation(wish), 620 * dt);
            Vector3 real = controller.velocity; real.y = 0;
            playerBody.moveSpeed = real.magnitude;
            playerBody.airborne = !grounded;
            // A fall out of the map would strand the save; bring the player back to a known point instead.
            if (player.position.y < (Location == Place.Home ? -160 : -20)) { var s = Mark(Location == Place.Home ? "M_SPAWN_home" : "M_SPAWN_lift"); TeleportPlayer(s.position, 90); }
        }

        void ApplyCursor(bool alt)
        {
            bool show = !inGame || overlay.Length > 0 || cp.phase == "action" || cp.phase == "complete" || alt;
            var mode = show ? CursorLockMode.None : CursorLockMode.Locked;
            if (Cursor.lockState != mode) Cursor.lockState = mode;
            if (Cursor.visible != show) Cursor.visible = show;
            cursorOn = show;
        }

        bool cursorOn;

        void TickCamera()
        {
            float dt = Time.unscaledDeltaTime;
            Vector3 pos; Quaternion rot;
            bool cinematic = TitleShot(out pos, out rot) || DialogueShot(out pos, out rot);
            if (!cinematic) FollowShot(dt, out pos, out rot);
            float k = cinematicBlend < 0 ? 1 : 1 - Mathf.Exp(-dt * (cinematic ? 5 : 12));
            cinematicBlend = 0;
            if (camSmoothPos == Vector3.zero) k = 1;
            camSmoothPos = Vector3.Lerp(camSmoothPos, pos, k);
            camSmoothRot = Quaternion.Slerp(camSmoothRot, rot, k);
            view.transform.SetPositionAndRotation(camSmoothPos, camSmoothRot);
        }

        void FollowShot(float dt, out Vector3 pos, out Quaternion rot)
        {
            var mouse = Mouse.current;
            if (mouse != null && !cursorOn && inGame && overlay.Length == 0 && !transitioning && cp.phase == "explore" && Time.timeScale > 0)
            {
                Vector2 delta = mouse.delta.ReadValue() * .085f * mouseSpeed;
                camYaw = Mathf.Repeat(camYaw + delta.x, 360);
                camPitch = Mathf.Clamp(camPitch - delta.y, -38, 62);
                float wheel = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(wheel) > .01f) camDistance = Mathf.Clamp(camDistance - Mathf.Sign(wheel) * .42f, 1.6f, 6.5f);
            }
            rot = Quaternion.Euler(camPitch, camYaw, 0);
            Vector3 target = player.position + Vector3.up * 1.42f;
            Vector3 back = rot * Vector3.back;
            float want = camDistance;
            if (Physics.SphereCast(target, .2f, back, out RaycastHit hit, want + .2f, ~(1 << PlayerLayer), QueryTriggerInteraction.Ignore))
                want = Mathf.Max(.55f, hit.distance - .12f);
            // Pull in fast so walls never clip, ease back out so the view does not pump.
            smoothDistance = want < smoothDistance ? Mathf.Lerp(smoothDistance, want, 1 - Mathf.Exp(-dt * 26)) : Mathf.Lerp(smoothDistance, want, 1 - Mathf.Exp(-dt * 4));
            pos = target + back * smoothDistance;
        }

        bool TitleShot(out Vector3 pos, out Quaternion rot)
        {
            pos = default; rot = default;
            if (inGame) return false;
            float t = reducedMotion ? 2 : titleClock;
            Vector3 c = Mark("M_TITLE_an").position;
            int shot = Mathf.FloorToInt(t / 20) % 2;
            float f = Mathf.Repeat(t, 20) / 20;
            if (shot == 0 || perf.Software)
            {
                // The painted menu, live: the three of them at the railing with the terraces behind.
                pos = c + new Vector3(Mathf.Lerp(-3.4f, -2.2f, f), 1.3f, -5.4f);
                rot = Quaternion.LookRotation(c + new Vector3(-3.2f, 2.3f, 16) - pos);
                return true;
            }
            // Flyover round the core of the dome.
            float a = Mathf.Lerp(-2.05f, -1.1f, f);
            pos = tramCentre + new Vector3(Mathf.Cos(a) * 395, -18 + f * 24, Mathf.Sin(a) * 395);
            rot = Quaternion.LookRotation(tramCentre + new Vector3(0, 8, 0) - pos);
            return true;
        }

        CiviBody ActorFor(string speaker)
        {
            switch (speaker)
            {
                case "Mai": return Location == Place.Ward ? bodies["mai_nho"] : playerBody;
                case "Linh": return bodies["linh"];
                case "Vinh": return bodies["vinh"];
                case "Mẹ Mai": return bodies["me_mai"];
                case "Hướng dẫn viên": return bodies["huong_dan"];
                case "Học sinh": return bodies["hoc_sinh"];
                case "Người phụ nữ áo xám": return bodies["ao_xam"];
                case "Nhân viên bệnh viện": return bodies["nv_benh_vien"];
                case "Đồng nghiệp cũ": return bodies["dong_nghiep_a"];
                default: return null;
            }
        }

        CiviBody lastSpeaker;

        bool DialogueShot(out Vector3 pos, out Quaternion rot)
        {
            pos = default; rot = default;
            if (cp == null || !cp.IsReading)
            {
                foreach (var body in bodies.Values) body.talking = false;
                lastSpeaker = null;
                return false;
            }
            int index = LineIndex;
            var speaker = ActorFor(content.speakers[index]);
            if (speaker != null && !speaker.gameObject.activeInHierarchy) speaker = null;
            foreach (var body in bodies.Values) body.talking = body == speaker && RevealLeft();
            if (Location == Place.Ward)
            {
                var ward = Mark("M_WARD_cam");
                pos = ward.position;
                var focus = (speaker ?? bodies["mai_nho"]).HeadPoint;
                rot = Quaternion.LookRotation(focus - pos);
                FaceEachOther(bodies["mai_nho"], speaker);
                return true;
            }
            if (speaker != null) lastSpeaker = speaker;
            var main = lastSpeaker != null ? lastSpeaker : playerBody;
            var other = main == playerBody ? NearestResidentBody() : playerBody;
            FaceEachOther(playerBody, other != playerBody ? other : null);
            if (other != null && other != playerBody) FaceEachOther(other, playerBody);
            Vector3 head = main.HeadPoint;
            Vector3 toOther = other != null && other != main ? other.HeadPoint - head : main.transform.forward;
            toOther.y = 0;
            if (toOther.sqrMagnitude < .01f) toOther = main.transform.forward;
            toOther.Normalize();
            Vector3 side = Vector3.Cross(Vector3.up, toOther);
            pos = head + toOther * 1.12f + side * .42f + Vector3.down * .03f;
            Vector3 aim = head + Vector3.down * .1f - side * .1f;
            if (Physics.Linecast(aim, pos, out RaycastHit hit, ~(1 << PlayerLayer), QueryTriggerInteraction.Ignore))
                pos = hit.point + (aim - pos).normalized * .2f;
            rot = Quaternion.LookRotation(aim - pos);
            return true;
        }

        CiviBody NearestResidentBody()
        {
            CiviBody best = null; float bestDistance = 36;
            foreach (var r in residents)
            {
                if (!r.on) continue;
                float d = (r.body.transform.position - player.position).sqrMagnitude;
                if (d < bestDistance) { bestDistance = d; best = r.body; }
            }
            return best;
        }

        void FaceEachOther(CiviBody who, CiviBody target)
        {
            if (who == null || target == null || who == target) return;
            who.hasLook = true;
            who.lookPoint = target.HeadPoint;
            Vector3 to = target.transform.position - who.transform.position;
            to.y = 0;
            if (to.sqrMagnitude > .02f)
                who.transform.rotation = Quaternion.RotateTowards(who.transform.rotation, Quaternion.LookRotation(to), 200 * Time.deltaTime);
        }

        void EnterTitle()
        {
            titleClock = 0;
            SetLocation(Place.Title);
            PlaceCast();
            SetMood();
            player.gameObject.SetActive(true);
            var m = Mark("M_TITLE_mai");
            TeleportPlayer(m.position, 168);
            playerBody.moveSpeed = 0;
            playerBody.expression = CiviBody.Smile;
            bodies["an"].expression = CiviBody.Happy;
            bodies["ha"].expression = CiviBody.Smile;
            bodies["an"].hasLook = true;
            bodies["an"].lookPoint = playerBody.HeadPoint;
            playerBody.hasLook = true;
            playerBody.lookPoint = bodies["an"].HeadPoint;
            drone.gameObject.SetActive(true);
            cat.gameObject.SetActive(true);
            cat.position = Mark("M_NPC_cat").position;
            hub.Ambience("C01_02_F_full_68s");
            SnapCamera();
        }
    }
}
