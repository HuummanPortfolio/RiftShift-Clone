using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace RiftShift_Clone.Test.GameTimer
{
    public class TimerTest : MonoBehaviour
    {
        /// <summary>
        /// Start/stop timer test
        /// Check isRunning
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator StartStopTimerTest()
        {
            var timerReference = FindObjectOfType<Timer>();
            Assert.IsNotNull(timerReference);

            float t = 0f;

            //Start Timer
            timerReference.OnTimerStart();
            while (t < 1)
            {
                t += Time.deltaTime;
                yield return null;
            }
            Assert.IsTrue(timerReference.isRunning == true);
            yield return null;

            t = 0f;
            //End Timer
            timerReference.OnTimerEnd();
            while (t < .1)
            {
                t += Time.deltaTime;
                yield return null;
            }

            Assert.IsTrue(timerReference.isRunning == false);
        }

        /// <summary>
        /// Pause Test
        /// Check isPaused
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator PauseTimerTest()
        {
            var timerReference = FindObjectOfType<Timer>();
            Assert.IsNotNull(timerReference);

            float t = 0f;

            //Pause Timer
            timerReference.OnTimerPause(true);
            while (t < .1)
            {
                t += Time.deltaTime;
                yield return null;
            }

            Assert.IsTrue(timerReference.isPaused == true);
            yield return null;

            t = 0f;
            //Unpause Timer
            timerReference.OnTimerPause(false);
            while (t < .1)
            {
                t += Time.deltaTime;
                yield return null;
            }

            Assert.IsTrue(timerReference.isPaused == false);
        }

        /// <summary>
        /// Timer test run in sequence
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator RunningTimer()
        {
            var t = 0f;

            Timer timerReference = FindObjectOfType<Timer>();
            Assert.IsNotNull(timerReference);

            //Start Timer
            t = 0f;
            timerReference.OnTimerStart();
            while (t < 1)
            {
                t += Time.deltaTime;
                yield return null;
            }
            Assert.IsTrue(timerReference.isRunning == true);
            yield return null;

            //Load Few Seconds
            t = 0f;
            while (t < 1)
            {
                t += Time.deltaTime;
                yield return null;
            }

            //Pause Timer
            t = 0f;
            timerReference.OnTimerPause(true);
            while (t < .1)
            {
                t += Time.deltaTime;
                yield return null;
            }

            //Load Few Seconds
            t = 0f;
            while (t < 1)
            {
                t += Time.deltaTime;
                yield return null;
            }

            //Unpause Timer
            t = 0f;
            timerReference.OnTimerPause(false);
            while (t < .1)
            {
                t += Time.deltaTime;
                yield return null;
            }

            //Load Few Seconds
            t = 0f;
            while (t < 1)
            {
                t += Time.deltaTime;
                yield return null;
            }

            //End Timer
            t = 0f;
            timerReference.OnTimerEnd();
            while (t < .1)
            {
                t += Time.deltaTime;
                yield return null;
            }
            Assert.IsTrue(timerReference.isRunning == false);


            yield return null;
        }
    }
}
