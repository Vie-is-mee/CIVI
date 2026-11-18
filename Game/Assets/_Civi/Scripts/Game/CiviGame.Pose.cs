using System.Collections;
using UnityEngine;
using Civi.Story;

namespace Civi
{
    // `-civi-qa <dir> -civi-pose`: shows one character in the real game at every speed and gesture and writes frames,
    // because character motion can only be judged through the real Animator.
    public sealed partial class CiviGame
    {
        IEnumerator PoseSheet()
        {
            titleOpenedAt = -10;
            StartNewGame();
            while (cp.IsReading && cp.phase == "intro")
            {
                lineShownAt = -100;
                yield return null;
                AdvanceDialogue();
                while (transitioning) yield return null;
                yield return null;
            }
            var at = new Vector3(-12, 0, -2);
            Goto(at, 0);
            camDistance = 2.2f;
            camPitch = 8;
            var who = bodies[qaPoseActor];
            who.gameObject.SetActive(true);
            who.transform.SetPositionAndRotation(at + new Vector3(1.1f, 0, 2.4f), Quaternion.Euler(0, -90, 0));
            yield return new WaitForSecondsRealtime(1);
            foreach (var (label, speed) in new[] { ("idle", 0f), ("walk", 1.27f), ("jog", 3.08f), ("run", 5.6f) })
            {
                who.moveSpeed = speed;
                yield return new WaitForSecondsRealtime(1.2f);
                for (int i = 0; i < 4; i++)
                {
                    yield return Shot("pose_" + label + "_" + i);
                    yield return new WaitForSecondsRealtime(.08f);
                }
            }
            who.moveSpeed = 0;
            who.transform.SetPositionAndRotation(at + new Vector3(1.1f, 0, 2.4f), Quaternion.Euler(0, 200, 0));
            yield return new WaitForSecondsRealtime(.8f);
            who.talking = true;
            yield return new WaitForSecondsRealtime(1.6f);
            for (int i = 0; i < 4; i++) { yield return Shot("pose_talk_" + i); yield return new WaitForSecondsRealtime(.5f); }
            who.talking = false;
            who.Gesture("reach");
            yield return new WaitForSecondsRealtime(.45f);
            yield return Shot("pose_pick_0");
            yield return new WaitForSecondsRealtime(.3f);
            yield return Shot("pose_pick_1");
            yield return new WaitForSecondsRealtime(2);
            who.airborne = true;
            yield return new WaitForSecondsRealtime(.2f);
            yield return Shot("pose_jump_0");
            who.airborne = false;
            yield return new WaitForSecondsRealtime(1);
            foreach (var (label, face) in new[] { ("smile", CiviBody.Smile), ("sad", CiviBody.Sad), ("surprised", CiviBody.Surprised), ("serious", CiviBody.Serious), ("happy", CiviBody.Happy) })
            {
                who.expression = face;
                yield return new WaitForSecondsRealtime(.5f);
                yield return Shot("pose_face_" + label);
            }
            who.expression = CiviBody.Neutral;
            // Close on Mai herself, in the dialogue camera the player sees most.
            camDistance = 1.6f;
            camPitch = 2;
            Goto(at, 180);
            camYaw = 0;
            yield return new WaitForSecondsRealtime(.6f);
            yield return Shot("pose_mai_face");
            camYaw = 40;
            yield return new WaitForSecondsRealtime(.6f);
            yield return Shot("pose_mai_three");
            camDistance = 3.4f;
            camPitch = 6;
            yield return new WaitForSecondsRealtime(.6f);
            yield return Shot("pose_mai_full");
            // The doorway between the office and the gallery, where the floor used to stop.
            Goto(new Vector3(-3.4f, 0, 1.2f), 110);
            camDistance = 3.6f;
            camPitch = 26;
            yield return new WaitForSecondsRealtime(.5f);
            yield return Shot("door_threshold");
        }

        string qaPoseActor = "ha"; // nobody else steers this one in the office

        // Plays one chapter start to finish through the same calls input uses, and stops at the first thing that does not work.
        IEnumerator EpisodeRun(int number)
        {
            BeginChapter(number);
            yield return null;
            while (transitioning) yield return null;
            Check(content.number == number && cp.chapter == number, "chapter " + number + " opens");
            hub.Fired = 0;
            int guard = 0;
            while (cp.phase != "complete")
            {
                Check(++guard < 4000, "chapter " + number + " keeps moving");
                string tag = "c" + number + "_" + cp.stage.ToString("00");
                if (cp.phase == "explore")
                {
                    Check(!Step.auto, tag + " waits for the player");
                    Check(Location == EpisodePlace(), tag + " is in the right place");
                    Goto(ObjectivePos() + Vector3.back * .4f, 0);
                    hub.Foot(FootBank(), .5f);
                    camPitch = 10;
                    yield return new WaitForSecondsRealtime(.3f);
                    yield return Shot(tag + "_go");
                    Check(focus != null, tag + " offers the objective");
                    focus.use();
                    yield return null;
                }
                else if (cp.phase == "action")
                {
                    int mask = ChapterCheckpoint.RequiredMask(Step.action);
                    for (int bit = 1; bit <= mask; bit <<= 1) ToggleInspection(bit);
                    yield return Shot(tag + "_action");
                    Check(CommitAction(), tag + " action commits");
                    yield return null;
                }
                else if (cp.IsReading)
                {
                    Check(hub.LayersPlaying > 0, tag + " has ambience");
                    if (cp.line == Mathf.Min(1, cp.Lines(content).Length - 1)) { yield return new WaitForSecondsRealtime(.5f); yield return Shot(tag + "_" + cp.phase); }
                    lineShownAt = -100;
                    yield return null;
                    AdvanceDialogue();
                    yield return null;
                }
                else yield return null;
                while (transitioning) yield return null;
            }
            Check(hub.Missing.Count == 0, "every sound of chapter " + number + " loads: " + string.Join(", ", hub.Missing));
            Check(hub.Fired == hub.Planned(number), "chapter " + number + " played all its cues: " + hub.Fired + "/" + hub.Planned(number));
            yield return Shot("c" + number + "_complete");
            Check(SaveCheckpoint() && ReadCheckpoint() != null && ReadCheckpoint().chapter == number, "chapter " + number + " saves");
            if (number == 2) yield return Sample("rain edge", 2);
        }

        IEnumerator ChaptersOnly()
        {
            titleOpenedAt = -10;
            StartNewGame();
            while (transitioning) yield return null;
            yield return EpisodeRun(2);
            yield return EpisodeRun(3);
            Check(ContinueGame() && content.number == 3 && cp.phase == "complete", "Continue restores chapter 3");
        }
    }
}
