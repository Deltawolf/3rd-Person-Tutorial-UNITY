using UnityEngine;
using System.Collections;

struct CameraPosition
{
	private Vector3 position;
	private Transform xForm;

	public Vector3 Position  { get {return position; } set {position = value;} }
	public Transform XForm { get { return xForm; } set { xForm = value; } }

	public void Init(string camName, Vector3 pos, Transform transform, Transform parent)
	{
		position = pos;
		xForm = transform;
		xForm.name = camName;
		xForm.parent = parent;
		xForm.localPosition = Vector3.zero;
		xForm.localPosition = position;
		
	}

}


[RequireComponent (typeof (BarsEffect))]
public class ThirdPersonCameraController: MonoBehaviour {
	
	#region Variables (private)
	[SerializeField]
	private PlayerControllerLogic follow;
	[SerializeField]
	private float distanceAway;
	[SerializeField]
	private float distanceUp;
	[SerializeField]
	private Transform followTransform;
	[SerializeField]
	private float firstPersonThreshold;
	[SerializeField]
	private Vector3 cameraOffset = new Vector3(0f ,1.5f, 0f);
	[SerializeField]
	private float widescreenTime = 0.2f;
	[SerializeField]
	private float targetingTime = 0.5f;
	

	private Vector3 velocityCamSmooth = Vector3.zero;
	[SerializeField]
	private float camSmoothDampTime = 0.1f;
	
	private Vector3 lookDir;
	private Vector3 targetPosition;
	private BarsEffect barEffect;
	private CamStates camState = CamStates.Behind;
	private float xAxisRot = 0.0f;
	private CameraPosition firstPersonCamPos;
	private float lookWeight;

	#endregion
	
	#region Properties (public)

	public enum CamStates 
	{
		Behind,
		FirstPerson,
		Target,
		Free
	}
	
	#endregion
	
	#region Unity events
	
	
	void Start ()
	{
		follow = GameObject.FindWithTag ("Player").GetComponent<PlayerControllerLogic> ();
		followTransform = GameObject.FindWithTag ("Player").transform;
		lookDir = followTransform.forward;

		barEffect = GetComponent<BarsEffect> ();
		if (barEffect == null)
		{
			Debug.LogError("Attach a widescreen BarsEffect script to the camera.", this);
		}

		firstPersonCamPos = new CameraPosition ();
		firstPersonCamPos.Init
			(
				"First Person Camera",
				new Vector3 (0.0f, 1.6f, 02f),
				new GameObject ().transform,
				followTransform

		);
	}


	void Update()
	{

	}
	
void LateUpdate()
	{
		float rightX = Input.GetAxis ("RightStickX");
		float rightY = Input.GetAxis ("RightStickY");
		float leftX = Input.GetAxis ("Horizontal");
		float leftY = Input.GetAxis ("Vertical");

		Vector3 characterOffset = followTransform.position + cameraOffset;

		if (Input.GetAxis ("Target") > 0.1f)
		{
			barEffect.coverage = Mathf.SmoothStep (barEffect.coverage, widescreenTime, targetingTime);

			camState = CamStates.Target;
		} 
		else
		{
			barEffect.coverage = Mathf.SmoothStep (barEffect.coverage, 0f, targetingTime);

			if (rightY > firstPersonThreshold && camState != CamStates.Free && !follow.IsInLocomotion())
			{
				xAxisRot = 0;
				lookWeight = 0f;
				camState = CamStates.FirstPerson;
			}

			camState = CamStates.Behind;
		}


		switch (camState) 
		{
			case CamStates.Behind:

				lookDir = characterOffset - this.transform.position;
				lookDir.y = 0;
				lookDir.Normalize ();
				Debug.DrawRay (this.transform.position, lookDir, Color.green);
				//Debug.DrawRay (follow.position, Vector3.up * distanceUp, Color.red);
				//Debug.DrawRay (follow.position, -1f * follow.forward * distanceAway, Color.blue);
				//Debug.DrawLine (follow.position, targetPosition, Color.magenta);
			break;

		case CamStates.Target:
			lookDir = followTransform.forward;
			break;
		}

		targetPosition = characterOffset + followTransform.up * distanceUp - lookDir * distanceAway;

		CompensateForWalls (characterOffset, ref targetPosition);

		smoothPosition (this.transform.position, targetPosition);


		transform.LookAt (followTransform);
	}
	
	#endregion Unity events
	
	#region Methods

	private void smoothPosition(Vector3 fromPos, Vector3 toPos)
	{
		this.transform.position = Vector3.SmoothDamp (fromPos, toPos, ref velocityCamSmooth, camSmoothDampTime);
	}

	private void CompensateForWalls(Vector3 fromObject, ref Vector3 toTarget)
	{
		Debug.DrawLine (fromObject, toTarget, Color.cyan);
		RaycastHit wallHit = new RaycastHit ();
		if (Physics.Linecast (fromObject, toTarget, out wallHit)) {
			Debug.DrawRay (wallHit.point, Vector3.left, Color.red);
			toTarget = new Vector3 (wallHit.point.x, toTarget.y, wallHit.point.z);
		}
	}
	#endregion Methods
	
}