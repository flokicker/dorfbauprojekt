using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechBranch
{
    public bool unlocked = false;
    public TechBranch parent = null;
    public List<TechBranch> children = new List<TechBranch>();
}
public class TechTree {
    public List<TechBranch> tree;

    public TechTree()
    {
        tree = new List<TechBranch>();

        // branch 1
        TechBranch branchOrigin = new TechBranch();
        TechBranch branch1 = new TechBranch();
        TechBranch branch2 = new TechBranch();
        branchOrigin.children.Add(branch1);
        branchOrigin.children.Add(branch2);
        branch1.parent = branchOrigin;
        branch2.parent = branchOrigin;

    }
}
