using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class StageInfoData
    {
        [SerializeField]
		private int _stage_idx;
		public int stage_idx
		{
			get { return _stage_idx;}
			set { _stage_idx = value;}
		}
		[SerializeField]
		private int _end_goal_value;
		public int end_goal_value
		{
			get { return _end_goal_value;}
			set { _end_goal_value = value;}
		}
		[SerializeField]
		private string _prefab;
		public string prefab
		{
			get { return _prefab;}
			set { _prefab = value;}
		}

    }

    [System.Serializable]
    public class StageInfo : Table<StageInfoData, int>
    {
    }
}

