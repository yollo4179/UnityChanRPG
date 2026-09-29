using UnityEngine;

public  class InstanceCreator :MonoBehaviour
{
    public  void AddInistanceByPath(string path,Transform  parent) {

        Managers.Resource.Instantiate(path, parent);
    }
    public virtual void AddInstances() { }
    public  virtual void AddPoolingInstances() { }
}
