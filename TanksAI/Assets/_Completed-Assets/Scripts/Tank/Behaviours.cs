using UnityEngine;
using UnityEngine.AI;
using NPBehave;
using System.Collections.Generic;

namespace Complete
{
    /*
    Example behaviour trees for the Tank AI.  This is partial definition:
    the core AI code is defined in TankAI.cs.

    Use this file to specifiy your new behaviour tree.
     */
    public partial class TankAI : MonoBehaviour
    {

		private System.Random rng;
		public Transform eyes;
		public NavMeshAgent navMeshAgent;

        private Root CreateBehaviourTree() {

            switch (m_Behaviour) {

				case 0:
					return FunBehaviour();
				case 1:
					return DeadlyBehaviour();
				case 2:
					return RunBehaviour();
				case 3:
					return CrazyBehaviour();

                default:
                    return new Root (new Action(()=> Turn(0.1f)));
            }
        }

        /* Actions */

        private Node StopTurning() {
            return new Action(() => Turn(0));
        }

        private Node RandomFire() {
            return new Action(() => Fire(UnityEngine.Random.Range(0.0f, 1.0f)));
        }

		private Node NonStopTurning(float turn, float shoot){
			return new Action(() => Turn(turn));
			return new Action(() => Fire (shoot));
		}


		private Node RangedFire ()
		{
			return new Action (() => ShotPrediction ());
		}

		private void ShotPrediction(){
			Vector3 targetPos = TargetTransform ().position;
			Vector3 localPos = this.transform.InverseTransformPoint (targetPos);
			float requiredVel = Mathf.Sqrt (localPos.magnitude* 15.0f);
			Debug.Log (requiredVel);
			float time = 0.0f;
			if (requiredVel > 25.0f)
				time = 1.0f;
			else if (requiredVel > 15.0f)
				time = (requiredVel -15.0f)/10.0f;
			Fire (time);
		}


        /* Example behaviour trees */

        // Constantly spin and fire on the spot 
        // private Root SpinBehaviour(float turn, float shoot) {
        //    return new Root(new Sequence(
        //                new Action(() => Turn(turn)),
        //                new Action(() => Fire(shoot))
        //            ));
        //}

        // Turn to face your opponent and fire
        // private Root TrackBehaviour() {
        //    return new Root(
        //       new Service(0.2f, UpdatePerception,
        //            new Selector(
        //                new BlackboardCondition("targetOffCentre",
        //                                        Operator.IS_SMALLER_OR_EQUAL, 0.1f,
        //                                        Stops.IMMEDIATE_RESTART,
        //                    // Stop turning and fire
        //                    new Sequence(StopTurning(),
        //                                new Wait(2f),
        //                                RandomFire())),
        //                new BlackboardCondition("targetOnRight",
        //                                        Operator.IS_EQUAL, true,
        //                                        Stops.IMMEDIATE_RESTART,
        //                    // Turn right toward target
        //                    new Action(() => Turn(0.2f))),
        //                    // Turn left toward target
        //                    new Action(() => Turn(-0.2f))
        //            )
        //        )
        //    );
        //}

		private Root CrazyBehaviour() {
			return new Root(
				new Service(0.2f, UpdatePerception,
					new Selector(
						new BlackboardCondition("targetOffCentre",
							Operator.IS_SMALLER_OR_EQUAL, 0.1f,
							Stops.IMMEDIATE_RESTART,
							// Stop turning and fire
							new Sequence(StopTurning(),
								new Wait(0.1f),
								RandomFire())),
						new BlackboardCondition("targetOnRight",
							Operator.IS_EQUAL, true,
							Stops.IMMEDIATE_RESTART,
							// Turn right toward target
							new Action(() => Turn(1f))),
						// Turn left toward target
						new Action(() => Turn(-1f))
					)
				)
			);
		}

		private Root DeadlyBehaviour () {
			return new Root(new Service(0.5f, UpdatePerception,
				new Sequence(
					new Action(() => Move((float) blackboard["move"])),
					new Selector(
						new BlackboardCondition("targetOffCentre",
							Operator.IS_SMALLER_OR_EQUAL, 0.1f,
							Stops.IMMEDIATE_RESTART,
							new Sequence(RangedFire())),
						new BlackboardCondition("targetOnRight",
							Operator.IS_EQUAL, true,
							Stops.IMMEDIATE_RESTART,
							// Turn right toward target
							new Action(() => Turn(1f))),
						// Turn left toward target
						new Action(() => Turn(-1f))
					)
				)));

		}

		private Root FunBehaviour () {
			return new Root(new Service(0.5f, UpdatePerception,
				new Sequence(
					new Action(() => Move((float) blackboard["move"])),
					new Selector(
						new BlackboardCondition("targetOffCentre",
							Operator.IS_SMALLER_OR_EQUAL, 0.1f,
							Stops.IMMEDIATE_RESTART,
							// Stop turning and fire
							new Sequence(StopTurning(),
								RandomFire())),
						new BlackboardCondition("targetOnRight",
							Operator.IS_EQUAL, true,
							Stops.IMMEDIATE_RESTART,
							// Turn right toward target
							new Action(() => Turn(1f))),
						// Turn left toward target
						new Action(() => Turn(-1f))
					)
				)));
		}

		private Root RunBehaviour () {
			return new Root(new Service(0.5f, UpdatePerception,
				new Sequence(
					new Action(() => Move((float) blackboard["move"])),
					new Selector(
						new BlackboardCondition("targetOffCentre",
							Operator.IS_SMALLER_OR_EQUAL, 0.1f,
							Stops.IMMEDIATE_RESTART,
							new Sequence(NonStopTurning(-1f,1f))),
						new BlackboardCondition("targetOnRight",
							Operator.IS_EQUAL, true,
							Stops.IMMEDIATE_RESTART,
							// Turn right toward target
							new Action(() => Turn(-1f))),
						// Turn left toward target
						new Action(() => Turn(1f))
					)
					)));

		}


        private void UpdatePerception() {

            Vector3 targetPos = TargetTransform().position;
            Vector3 localPos = this.transform.InverseTransformPoint(targetPos);
            Vector3 heading = localPos.normalized;

            blackboard["targetDistance"] = localPos.magnitude;
            blackboard["targetInFront"] = heading.z > 0;
            blackboard["targetOnRight"] = heading.x > 0;
            blackboard["targetOffCentre"] = Mathf.Abs(heading.x);
		
			if (rng == null) {
				rng = new System.Random();
			}
			blackboard["turn"] = (float) randomFloat();
			blackboard["move"] = (float) rng.NextDouble();
		}

		float randomFloat() {
			float r = (float) rng.NextDouble();
			return -1f + r*2f;
		}


        }

    }
