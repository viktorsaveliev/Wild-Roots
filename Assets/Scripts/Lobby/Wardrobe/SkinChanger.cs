/*using System.Collections.Generic;
using UnityEngine;

public class SkinChanger : MonoBehaviour
{
    [SerializeField] private MeshRenderer _headMesh;
    private List<Material> _headMaterials = new();

    [SerializeField] private MeshRenderer _bodyMesh;
    private List<Material> _bodyMaterials = new();

    [SerializeField] private MeshRenderer[] _handsAndFoots;
    private List<Material> _bodyLimbsMaterial = new();
    private List<Material> _bodyLimbsWoolMaterial = new();

    [SerializeField] private Material _testMaterial;

    public enum HeadParts
    {
        Ear,
        EarInside,
        Heat,
        Wool
    }

    public enum BodyParts
    {
        Nose,
        Body,
        Eyes,
        Suit,
        Dots,
        Pockets,
        Belt
    }

    private void Start()
    { 
        _bodyMesh.GetMaterials(_bodyMaterials);
        _headMesh.GetMaterials(_headMaterials);

        for (int i = 0; i < _handsAndFoots.Length; i++)
        {
            List<Material> materials = new();
            _handsAndFoots[i].GetMaterials(materials);

            _bodyLimbsMaterial.Add(materials[0]);
            _bodyLimbsWoolMaterial.Add(materials[1]);
        }
    }

    public void SetBodyMaterial(Material material, BodyParts part)
    {
        if(part == BodyParts.Body)
        {
            for (int i = 0; i < _handsAndFoots.Length; i++)
            {
                if(i == 4) // if head ear
                {
                    SetHeadMaterial(material, HeadParts.Ear);
                    continue;
                }
                List<Material> materials = new()
                {
                    material,
                    _bodyLimbsWoolMaterial[i]
                };
                _handsAndFoots[i].SetMaterials(materials);
            }
        }
        _bodyMaterials[(int)part] = material;

        _bodyMesh.SetMaterials(_bodyMaterials);
    }

    public void SetHeadMaterial(Material material, HeadParts part)
    {
        _headMaterials[(int)part] = material;
        _headMesh.SetMaterials(_headMaterials);
    }
}*/