using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("GameSettings")]
    [SerializeField] float _health;
    [SerializeField] float _maxhealth;
    [Header("Material")]
    [SerializeField] Material damageMaterial;
    List<List<Material>> _defaultMaterials; // Her MeshRenderer i�in materyal listesi
    [SerializeField] Renderer[] _renderers; // T�m mesh rendererlar� tutacak

    private void Awake()
    {
        _health = _maxhealth;

        // Her MeshRenderer'�n materyallerini saklamak
        _defaultMaterials = new List<List<Material>>();
        foreach (var renderer in _renderers)
        {
            _defaultMaterials.Add(new List<Material>(renderer.materials));
        }
    }
    public void SetDamage(float _damage)
    {
        _health -= _damage;
        Debug.Log(name + " damaged by the enemey");
        StartCoroutine(ChangeMaterial(damageMaterial));
        if (_health <= 0)
        {
            _health = 0;
            Debug.Log(name + " �ld�.");
            SpawnManager.instance.FaultyAIResolution(this.transform);
            Destroy(gameObject);
            SpawnManager.instance.SpawnEnemies(1);
            //Olum islemleri...
        }
    }
    IEnumerator ChangeMaterial(Material material)
    {
        // T�m MeshRenderer'larda materyali de�i�tirme
        for (int i = 0; i < _renderers.Length; i++)
        {
            Renderer renderer = _renderers[i];
            Material[] materials = renderer.materials;

            // Her materyali de�i�tiriyoruz
            for (int j = 0; j < materials.Length; j++)
            {
                materials[j] = material;
            }

            renderer.materials = materials; // De�i�iklikleri uyguluyoruz
        }

        yield return new WaitForSeconds(0.2f);

        // Eski materyalleri geri y�klemek
        for (int i = 0; i < _renderers.Length; i++)
        {
            Renderer renderer = _renderers[i];
            Material[] materials = renderer.materials;

            // Eski materyalleri geri y�kle
            for (int j = 0; j < materials.Length; j++)
            {
                materials[j] = _defaultMaterials[i][j]; // Her mesh i�in eski materyali geri y�kl�yoruz
            }

            renderer.materials = materials;
        }
    }


    public bool IsDead()
    {
        return _health <= 0;
    }
}
