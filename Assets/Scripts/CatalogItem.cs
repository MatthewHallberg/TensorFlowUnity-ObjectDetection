using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatalogItem {
	
	public int Id { get; set; }
	public string Name { get; set; }
	public string DisplayName { get; set; }
	public Rect Box { get; set; }
	public float Score { get; set; }
}
