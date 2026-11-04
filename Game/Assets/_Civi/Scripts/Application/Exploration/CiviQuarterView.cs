using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Civi.Preview
{
    // Orthographic 2D: the 3/4 appearance comes from sprites/foreground layers,
    // NOT from rotating the camera like a 3D game.
    [RequireComponent(typeof(Camera))]
    public sealed class CiviFollowCamera2D : MonoBehaviour
    {
        public Transform Target;
        public Rect WorldBounds = new Rect(-9f, -5.65f, 18f, 11.3f);
        public float SmoothTime = .22f;
        public float LookAheadDistance = .40f;
        public float VerticalComposition = .24f;
        public float MinZoom = 3.1f;
        public float MaxZoom = 4.45f;

        Camera camera2D;
        Vector3 velocity;
        Vector2 lookAhead;
        float desiredZoom = 3.75f;

        public void Configure(Transform target, Rect bounds, float zoom)
        {
            camera2D = GetComponent<Camera>();
            camera2D.orthographic = true;
            Target = target;
            WorldBounds = bounds;
            desiredZoom = Mathf.Clamp(zoom, MinZoom, MaxZoom);
            camera2D.orthographicSize = desiredZoom;
            lookAhead = Vector2.zero;
            velocity = Vector3.zero;
            Snap();
        }

        public void Snap()
        {
            if (Target == null) return;
            if (camera2D == null) camera2D = GetComponent<Camera>();
            Vector3 targetPos = Target.position + new Vector3(0f, VerticalComposition, 0f);
            transform.position = Bounded(targetPos);
            velocity = Vector3.zero;
        }

        void LateUpdate()
        {
            if (Target == null) return;
            if (camera2D == null) camera2D = GetComponent<Camera>();
            if (Mouse.current != null && Mouse.current.scroll.ReadValue().y != 0f)
                desiredZoom = Mathf.Clamp(desiredZoom - Mathf.Sign(Mouse.current.scroll.ReadValue().y) * .22f, MinZoom, MaxZoom);
            camera2D.orthographicSize = Mathf.MoveTowards(camera2D.orthographicSize, desiredZoom, Time.deltaTime * 4.5f);

            var mover = Target.GetComponent<Civi2DPlayer>();
            Vector2 input = mover == null ? Vector2.zero : mover.MoveDirection;
            lookAhead = Vector2.Lerp(lookAhead, input * LookAheadDistance, 1f - Mathf.Exp(-Time.deltaTime * 7f));
            Vector3 next = Target.position + (Vector3)lookAhead + new Vector3(0f, VerticalComposition, 0f);
            transform.position = Vector3.SmoothDamp(transform.position, Bounded(next), ref velocity, SmoothTime);
        }

        Vector3 Bounded(Vector3 target)
        {
            float halfH = camera2D.orthographicSize;
            float halfW = halfH * camera2D.aspect;
            float minX = WorldBounds.xMin + halfW;
            float maxX = WorldBounds.xMax - halfW;
            float minY = WorldBounds.yMin + halfH;
            float maxY = WorldBounds.yMax - halfH;
            float x = minX > maxX ? WorldBounds.center.x : Mathf.Clamp(target.x, minX, maxX);
            float y = minY > maxY ? WorldBounds.center.y : Mathf.Clamp(target.y, minY, maxY);
            return new Vector3(x, y, -10f);
        }
    }

    // Every actor and piece of tall furniture is layered by its FOOT position.
    // This lets An appear in front of desks when standing below them.
    [RequireComponent(typeof(SortingGroup))]
    public sealed class CiviDepthSort2D : MonoBehaviour
    {
        public float FeetOffset;
        SortingGroup group;
        void Awake() { group = GetComponent<SortingGroup>(); }
        void LateUpdate()
        {
            if (group == null) group = GetComponent<SortingGroup>();
            group.sortingOrder = 1200 - Mathf.RoundToInt((transform.position.y + FeetOffset) * 100f);
        }
        public void ApplyNow()
        {
            if (group == null) group = GetComponent<SortingGroup>();
            group.sortingOrder = 1200 - Mathf.RoundToInt((transform.position.y + FeetOffset) * 100f);
        }
    }

    // Replaceable placeholder for TV3's final four-direction sprites / Animator.
    public sealed class CiviActorVisual2D : MonoBehaviour
    {
        Transform visual;
        GameObject frontEyes;
        GameObject backHighlight;
        Civi2DPlayer controller;
        Vector2 facing = Vector2.down;
        Vector3 idlePosition;

        static GameObject Piece(Transform parent, string name, Vector2 pos, Vector2 size, Color color, int sorting)
        {
            var go = new GameObject(name, typeof(SpriteRenderer));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = CiviQuarterWorld2D.Pixel;
            sr.color = color;
            sr.sortingOrder = sorting;
            return go;
        }

        public void Build()
        {
            controller = GetComponent<Civi2DPlayer>();
            var body = GetComponent<SpriteRenderer>();
            if (body != null) body.color = Color.clear;
            if (GetComponent<SortingGroup>() == null) gameObject.AddComponent<SortingGroup>();
            if (GetComponent<CiviDepthSort2D>() == null) gameObject.AddComponent<CiviDepthSort2D>();
            var shadow = Piece(transform, "Ground shadow", new Vector2(0f, -.32f),
                new Vector2(.75f,.16f),new Color(.13f,.20f,.17f,.34f),-6);
            shadow.transform.localPosition = new Vector3(0,-.32f,0);
            visual = new GameObject("AN / layered 3-quarter actor").transform;
            visual.SetParent(transform,false);
            visual.localPosition=Vector3.zero;
            idlePosition=visual.localPosition;
            Piece(visual,"Shoes",new Vector2(0f,-.30f),new Vector2(.42f,.20f),new Color(.21f,.24f,.28f),0);
            Piece(visual,"Backpack",new Vector2(-.21f,-.02f),new Vector2(.28f,.48f),new Color(.69f,.41f,.25f),1);
            Piece(visual,"School shirt",new Vector2(0f,-.015f),new Vector2(.53f,.49f),new Color(.26f,.57f,.54f),2);
            Piece(visual,"Neck",new Vector2(0f,.30f),new Vector2(.20f,.16f),new Color(.92f,.69f,.48f),3);
            Piece(visual,"Face",new Vector2(0f,.44f),new Vector2(.43f,.39f),new Color(.93f,.73f,.57f),4);
            Piece(visual,"Hair",new Vector2(0f,.66f),new Vector2(.49f,.18f),new Color(.19f,.24f,.25f),5);
            frontEyes = Piece(visual,"Eyes",new Vector2(0f,.45f),
                new Vector2(.19f,.038f),new Color(.18f,.26f,.28f),6);
            backHighlight = Piece(visual,"Hair from rear",new Vector2(0f,.46f),
                new Vector2(.44f,.33f),new Color(.19f,.24f,.25f),6);
            backHighlight.SetActive(false);
        }

        void Update()
        {
            if (controller == null || visual == null) return;
            Vector2 direction = controller.MoveDirection;
            bool moving = direction.sqrMagnitude > .03f;
            if (moving) facing=direction;
            bool facingBack = Mathf.Abs(facing.y) > Mathf.Abs(facing.x) && facing.y > 0f;
            frontEyes.SetActive(!facingBack);
            backHighlight.SetActive(facingBack);
            float bob = moving ? Mathf.Sin(Time.time*11f)*.026f : Mathf.Sin(Time.time*2.5f)*.009f;
            visual.localPosition = idlePosition + new Vector3(0,bob,0);
            float turn = Mathf.Abs(facing.x) > Mathf.Abs(facing.y) ? Mathf.Sign(facing.x) : 0f;
            visual.localScale = new Vector3(turn == 0f ? 1f : turn,1f,1f);
        }
    }

    // Two distinct greybox stages, sharing a single canonical S02/S03 StorySession.
    // Their geometry is temporary; approved sprites or Tilemaps can replace it.
    public sealed class CiviQuarterWorld2D : MonoBehaviour
    {
        static Sprite pixel;
        public static Sprite Pixel
        {
            get
            {
                if (pixel == null)
                    pixel = Sprite.Create(Texture2D.whiteTexture, new Rect(0f,0f,1f,1f),
                        new Vector2(.5f,.5f),1f);
                return pixel;
            }
        }

        GameObject s02, s03;
        static readonly Color dark = new Color(.15f,.27f,.28f);
        static readonly Color wood = new Color(.43f,.33f,.28f);
        static readonly Color floor = new Color(.85f,.82f,.73f);
        static readonly Color teal = new Color(.29f,.52f,.49f);
        static readonly Color cream = new Color(.97f,.92f,.79f);

        public void Build(Transform parent)
        {
            transform.SetParent(parent,false);
            s02=BuildRoom("S02 / GÓC KIỂM ĐỒ",new Color(.81f,.84f,.76f),true);
            s03=BuildRoom("S03 / PHÒNG ẢNH",new Color(.83f,.80f,.74f),false);
            SetStage("S02");
        }
        public void SetStage(string id)
        {
            if(s02!=null)s02.SetActive(id=="S02");
            if(s03!=null)s03.SetActive(id=="S03");
        }

        static GameObject Plate(Transform parent,string name,Vector2 position,Vector2 scale,Color tint,int order)
        {
            var node=new GameObject(name,typeof(SpriteRenderer));
            node.transform.SetParent(parent,false);
            node.transform.localPosition=position;
            node.transform.localScale=new Vector3(scale.x,scale.y,1f);
            var sr=node.GetComponent<SpriteRenderer>();
            sr.sprite=Pixel;sr.color=tint;sr.sortingOrder=order;
            return node;
        }

        static void Wall(Transform parent,string id,Vector2 center,Vector2 size,Color color)
        {
            var wall=Plate(parent,id,center,size,color,-25);
            wall.AddComponent<BoxCollider2D>();
        }

        static void Table(Transform parent,string name,Vector2 at,Vector2 size,Color color)
        {
            var root=new GameObject(name);
            root.transform.SetParent(parent,false);root.transform.localPosition=at;
            root.AddComponent<SortingGroup>();
            var depth=root.AddComponent<CiviDepthSort2D>();depth.FeetOffset=-.60f;
            Plate(root.transform,"Shadow",new Vector2(.15f,-.45f),new Vector2(size.x+.22f,.55f),dark*.7f,-5);
            Plate(root.transform,"Legs",new Vector2(0f,-.45f),new Vector2(size.x-.30f,.46f),wood*.65f,0);
            Plate(root.transform,"Front face",new Vector2(0f,-.29f),new Vector2(size.x,.39f),wood,1);
            Plate(root.transform,"Top surface",new Vector2(0f,.08f),size,color,2);
            Plate(root.transform,"Highlight",new Vector2(0f,.28f),new Vector2(size.x-.20f,.065f),cream*.87f,3);
            var collider=root.AddComponent<BoxCollider2D>();
            collider.offset=new Vector2(0f,-.01f);
            collider.size=new Vector2(size.x,.82f);
            depth.ApplyNow();
        }

        static void Decor(Transform parent,string name,Vector2 at,Vector2 size,Color color,bool dim=false)
        {
            Plate(parent,name+" drop shadow",at+new Vector2(.10f,-.12f),size, dark*.60f,-24);
            Plate(parent,name,at,size,color,dim?-12:-10);
        }

        GameObject BuildRoom(string id,Color tiles,bool equipment)
        {
            var room=new GameObject(id);
            room.transform.SetParent(transform,false);
            Plate(room.transform,"Floor / warm paper",Vector2.zero,new Vector2(18.5f,11.5f),tiles,-600);
            // Raised rear wall and floor-strip shadows give a 3/4 perspective.
            Plate(room.transform,"Rear wall face",new Vector2(0,4.88f),new Vector2(18.5f,1.65f),
                equipment?new Color(.42f,.63f,.59f):new Color(.48f,.57f,.62f),-90);
            Plate(room.transform,"Rear wall top",new Vector2(0,5.72f),new Vector2(18.5f,.24f),dark,-89);
            Plate(room.transform,"Rear wall skirting",new Vector2(0,4.04f),new Vector2(18.5f,.14f),wood*.77f,-88);
            for(int i=-4;i<=4;i++)
            {
                Plate(room.transform,"Floorboard row "+i,new Vector2(0,i*1.05f-.15f),
                    new Vector2(17.6f,.024f),new Color(.39f,.41f,.34f,.22f),-580);
            }
            for(int i=-7;i<=7;i++)
                Plate(room.transform,"Floor seam "+i,new Vector2(i*1.13f,.0f),
                    new Vector2(.018f,9.0f),new Color(.57f,.54f,.47f,.16f),-580);
            Wall(room.transform,"Left boundary",new Vector2(-9.07f,0),new Vector2(.35f,11.2f),dark);
            Wall(room.transform,"Right boundary",new Vector2(9.07f,0),new Vector2(.35f,11.2f),dark);
            Wall(room.transform,"North boundary",new Vector2(0,5.42f),new Vector2(18.5f,.28f),dark);
            Wall(room.transform,"South boundary",new Vector2(0,-5.55f),new Vector2(18.5f,.32f),dark);
            // Rear wall dressing never blocks critical evidence.
            Decor(room.transform,"Wallboard",new Vector2(-5.5f,4.67f),
                new Vector2(3.15f,.88f),cream);
            Decor(room.transform,"Window",new Vector2(5.55f,4.66f),
                new Vector2(2.25f,.95f),new Color(.68f,.84f,.84f));
            Plate(room.transform,"Sunlight",new Vector2(5.55f,4.57f),
                new Vector2(2.0f,.08f),cream*.95f,-9);
            Table(room.transform,equipment?"Equipment check counter":"Photo development desk",
                new Vector2(-1.48f,2.87f),new Vector2(5.45f,.75f),
                equipment?new Color(.64f,.56f,.44f):new Color(.61f,.49f,.39f));
            Table(room.transform,"Secondary worktable",
                new Vector2(5.25f,2.87f),new Vector2(2.22f,.75f),
                new Color(.66f,.55f,.43f));
            if(equipment)
            {
                Decor(room.transform,"Tool mat",new Vector2(-5.7f,-3.6f),
                    new Vector2(2.15f,1.22f),new Color(.63f,.70f,.60f));
                Decor(room.transform,"Open storage rack",new Vector2(6.55f,-3.8f),
                    new Vector2(1.65f,.57f),new Color(.55f,.43f,.34f));
            }
            else
            {
                Decor(room.transform,"Storyboard on floor",new Vector2(-4.95f,-3.70f),
                    new Vector2(2.45f,1.16f),cream);
                Decor(room.transform,"Projector case",new Vector2(5.75f,-3.45f),
                    new Vector2(2.05f,.75f),new Color(.49f,.53f,.53f));
            }
            return room;
        }
    }
}
