using NUnit.Framework;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AnimatorCom : MonoBehaviour
{

    private Animator _animator;
    public Animator _Animator { get=> _animator;}

    protected virtual void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    
   
}
