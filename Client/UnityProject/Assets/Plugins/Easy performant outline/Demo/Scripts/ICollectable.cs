using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EPOOutline.Demo
{
    public interface ICollectable
    {
        void Collect(GameObject collector);
    }
}