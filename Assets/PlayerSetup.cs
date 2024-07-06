using Photon.Pun;
using TMPro;
using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    public Movement movement;

    //public GameObject camera;

    public GameObject playerTag;

    public string nickname;

    public TextMeshPro nicknameText;

    //public Transform tpweaponholder;

    //public Transform weaponHolder;

    public void isLocalPlayer()
    {
        //tpweaponholder.gameObject.SetActive(false);
        movement.enabled = true;
        //camera.SetActive(true);
        playerTag.SetActive(false);
    }


    //This maybe usse for player number
    /*    [PunRPC]
        public void setTPWeapon(int _weaponIndex)
        {
            foreach (Transform _child in tpweaponholder)
            {
                _child.gameObject.SetActive(false);
            }

            tpweaponholder.GetChild(_weaponIndex).gameObject.SetActive(true);
        }*/

    [PunRPC]
    public void SetName(string _name)
    {
        nickname = _name;
        nicknameText.text = nickname;
    }


}
