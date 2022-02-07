using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Photon.Pun;

namespace GameModeCollection.Extensions
{
    public static class MapManagerExtensions
    {
        public static void LoadSpecificLevel(this MapManager instance, string level)
        {
            instance.GetComponent<PhotonView>().RPC("RPCA_LoadLevel", RpcTarget.All, level);

        }
    }
}
