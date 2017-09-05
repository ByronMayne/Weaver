using UnityEngine;

namespace Weaver
{
    [CreateAssetMenu(menuName = "Weaver/Extensions/Profile Sample", fileName = "Profile Sample Extension")]
    public class ProfileSampleExtension : WeaverExtension
    {
        public override string addinName
        {
            get
            {
                return "Profile Sample";
            }
        }
    }
}
