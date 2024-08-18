using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//IMPORTANTE: Esta clase esta completa, NO LA DEBE MODIFICAR -- USELA ASI
public class Cell : MonoBehaviour {

	public const int EMPTY = 0;

	public Text label;
	
	int _number;
	bool _locked;
	bool _invalid;
	Image _image;
	Color _prev;

	public int number {
		get { return _number; }
		set {
			_number = value;
			if(value == EMPTY)
				label.text = "";
			else
				label.text = value.ToString();
		}
	}

	public bool isEmpty {
		get { return _number == 0; }
	}

	public bool invalid {
		get { return _invalid; }
		set {
			_invalid = value;
			RefreshColor();
		}
	}

	public bool locked {
		get { return _locked; }
		set {
			_locked = value;
			RefreshColor();
		}
	}

	void RefreshColor() {
		if(_invalid)
			_image.color = Color.red;
		else if(_locked)
			_image.color = new Color(0.75f, 0.75f, 0.75f);
		else
			_image.color = Color.white;
	}

	// Use this for initialization
	void Awake() {
		//label.text = "";
		_image = GetComponent<Image>();
		RefreshColor();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
