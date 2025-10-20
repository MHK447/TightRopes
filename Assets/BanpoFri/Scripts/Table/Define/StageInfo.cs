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
		[SerializeField]
		private int _base_balance_value;
		public int base_balance_value
		{
			get { return _base_balance_value;}
			set { _base_balance_value = value;}
		}
		[SerializeField]
		private float _base_sway_value;
		public float base_sway_value
		{
			get { return _base_sway_value;}
			set { _base_sway_value = value;}
		}
		[SerializeField]
		private float _product_sway_value;
		public float product_sway_value
		{
			get { return _product_sway_value;}
			set { _product_sway_value = value;}
		}
		[SerializeField]
		private int _end_balance_value;
		public int end_balance_value
		{
			get { return _end_balance_value;}
			set { _end_balance_value = value;}
		}
		[SerializeField]
		private float _end_sway_value;
		public float end_sway_value
		{
			get { return _end_sway_value;}
			set { _end_sway_value = value;}
		}
		[SerializeField]
		private List<int> _cam_rot;
		public List<int> cam_rot
		{
			get { return _cam_rot;}
			set { _cam_rot = value;}
		}
		[SerializeField]
		private string _image;
		public string image
		{
			get { return _image;}
			set { _image = value;}
		}
		[SerializeField]
		private string _name;
		public string name
		{
			get { return _name;}
			set { _name = value;}
		}
		[SerializeField]
		private string _image_color;
		public string image_color
		{
			get { return _image_color;}
			set { _image_color = value;}
		}
		[SerializeField]
		private string _product_img;
		public string product_img
		{
			get { return _product_img;}
			set { _product_img = value;}
		}

    }

    [System.Serializable]
    public class StageInfo : Table<StageInfoData, int>
    {
    }
}

