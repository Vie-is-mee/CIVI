using System;
using UnityEngine;

namespace Civi.Preview
{
    // The only mutable data carried across a door-triggered scene transition.
    // No old yearbook or Lưu Quang campaign state is modified.
    public static class CiviSchoolV2Bridge
    {
        public const string CampusScene="CIVI-School-VisualV2";
        public const string StudioScene="CIVI-School-DesignStudio-V2";
        public static Vector2 ReturnPosition;
        public static string[] Explored=Array.Empty<string>();
        public static bool Returning=false;
    }
}
