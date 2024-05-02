using CNCO.Unify.Events;
using System.Diagnostics;

namespace UnifyTests.Events {
    public class EventEmitterTests {
        private static IEventEmitter EventEmitter;

        private static int CallbackHitCount = 0;
        private static object?[]? CallbackOptions = null;
        private static readonly Callback EventCallback = new Callback((options) => {
            Debug.WriteLine(options);
            CallbackHitCount++;
            CallbackOptions = options;
        });

        private static readonly string TestEventName = "TestEvent";

        [SetUp]
        public void Setup() {
            EventEmitter = new EventEmitter();
            CallbackHitCount = 0;
            CallbackOptions = null;
        }

        [TearDown]
        public void TearDown() {
            EventEmitter.Dispose();
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(6)]
        [TestCase(-1)] // max
        public void EventsCount_WithNEvents_ReturnsN(int numberOfEvents) {
            if (numberOfEvents < 0)
                numberOfEvents = EventEmitter.GetMaxEvents();
            numberOfEvents = Math.Min(numberOfEvents, EventEmitter.GetMaxEvents());

            // Setup
            for (int i = 0; i < numberOfEvents; i++)
                EventEmitter.AddListener(Guid.NewGuid().ToString(), EventCallback);

            // Evaluate
            Assert.That(EventEmitter.EventsCount(), Is.EqualTo(numberOfEvents));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(3)]
        [TestCase(-1)] // max
        public void ListenersCount_WithNEvents_ReturnsN(int numberOfListeners) {
            if (numberOfListeners < 0)
                numberOfListeners = EventEmitter.GetMaxListeners();
            numberOfListeners = Math.Min(numberOfListeners, EventEmitter.GetMaxListeners());

            // Setup
            for (int i = 0; i < numberOfListeners; i++)
                EventEmitter.AddListener(TestEventName, EventCallback);

            Assert.That(EventEmitter.ListenersCount(TestEventName), Is.EqualTo(numberOfListeners));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(3)]
        [TestCase(-1)] // max
        public void ListenersCount_SpecifyEventCallBackWithNEvents_ReturnsN(int numberOfListeners) {
            if (numberOfListeners < 0)
                numberOfListeners = EventEmitter.GetMaxListeners() - 1;
            numberOfListeners = Math.Min(numberOfListeners, EventEmitter.GetMaxListeners() - 1);

            // Setup
            for (int i = 0; i < numberOfListeners; i++)
                EventEmitter.AddListener(TestEventName, EventCallback);

            // One extra, different callback
            EventEmitter.AddListener(TestEventName, new Callback((options) => { }));

            // Check we add N listeners for EventCallback (which should be 1 less than the total listeners for the event)
            Assert.That(EventEmitter.ListenersCount(TestEventName, EventCallback), Is.EqualTo(numberOfListeners));
        }


        public enum AddMethod {
            AddListener,
            On,
            Once,
            PrependListener,
            PrependOnceListener
        }
        public enum RemoveMethod {
            Off,
            RemoveListener,
            RemoveAllListeners
        }

        [TestCase("TestEvent")]
        [TestCase("TestEvent", AddMethod.On)]
        [TestCase("TestEvent", AddMethod.Once)]
        [TestCase("TestEvent", AddMethod.PrependListener)]
        [TestCase("TestEvent", AddMethod.PrependOnceListener)]
        [TestCase("my.super<>cool!event?")]
        [TestCase("my.super<>cool!event?", AddMethod.PrependOnceListener)]
        [TestCase("This_is_an_event_name_that_does_not_exceed_128_characters_and_should_be_considered_invalid_but_gets_very_close_to_it_0123456789")]
        public void AddListener_ValidName_AddsEventListener(string eventName, AddMethod addMethod = AddMethod.AddListener) {
            bool shouldBeOnTimer = false;
            switch (addMethod) {
                case AddMethod.On:
                    EventEmitter.On(eventName, EventCallback);
                    break;

                case AddMethod.Once:
                    shouldBeOnTimer = true;
                    EventEmitter.Once(eventName, EventCallback);
                    break;

                case AddMethod.PrependListener:
                    EventEmitter.PrependListener(eventName, EventCallback);
                    break;

                case AddMethod.PrependOnceListener:
                    shouldBeOnTimer = true;
                    EventEmitter.PrependOnceListener(eventName, EventCallback);
                    break;

                case AddMethod.AddListener:
                default:
                    EventEmitter.AddListener(eventName, EventCallback);
                    break;
            }

            IEventEmitterListener[] listeners = EventEmitter.Listeners(eventName);

            Assert.Multiple(() => {
                // Only one
                Assert.That(EventEmitter.ListenersCount(eventName), Is.EqualTo(1));

                // Only one
                Assert.That(listeners, Has.Length.EqualTo(1));

                // If once, verify
                bool addedAsOneTimer = listeners.First().GetOneTimeListener();
                Assert.That(addedAsOneTimer, Is.EqualTo(shouldBeOnTimer));
            });
        }

        [TestCase(RemoveMethod.Off)]
        [TestCase(RemoveMethod.RemoveListener)]
        [TestCase(RemoveMethod.RemoveAllListeners)]
        public void RemoveListener_ValidEventListener_RemovesListener(RemoveMethod removeMethod) {
            int eventsThatShouldRemain = 1;
            EventEmitter.AddListener(TestEventName, EventCallback);
            EventEmitter.AddListener(TestEventName, new Callback());

            // Remove
            switch (removeMethod) {
                case RemoveMethod.RemoveListener:
                    EventEmitter.RemoveListener(TestEventName, EventCallback);
                    break;

                case RemoveMethod.RemoveAllListeners:
                    EventEmitter.RemoveAllListeners(TestEventName);
                    eventsThatShouldRemain = 0;
                    break;

                case RemoveMethod.Off:
                default:
                    EventEmitter.Off(TestEventName, EventCallback);
                    break;
            }

            // Verify it got removed
            IEventEmitterListener[] listeners = EventEmitter.Listeners(TestEventName);

            Assert.Multiple(() => {
                Assert.That(listeners, Has.Length.EqualTo(eventsThatShouldRemain));
                Assert.That(EventEmitter.ListenersCount(TestEventName), Is.EqualTo(eventsThatShouldRemain));
            });
        }

        [TestCase(null)]
        [TestCase("a string")]
        [TestCase("a string and number", 1)]
        [TestCase(false, "whatever this is", new int[] { 0, 1, 2 })]
        public void Emit_WithEventArguments_ActivatesEvent(object?[]? eventArguments) {
            EventEmitter.AddListener(TestEventName, EventCallback);

            // Call event
            EventEmitter.Emit(TestEventName, eventArguments);

            Assert.Multiple(() => {
                // Verify hit count
                Assert.That(CallbackHitCount, Is.EqualTo(1));

                // Verify arguments
                Assert.That(CallbackOptions, Is.EqualTo(eventArguments));
            });
        }


        // Verify case insensitivity
        [TestCase("TestEvent")]
        [TestCase("mYNotSoEASy2ReADevntNamE")]
        [TestCase("my.super<>cool!event?")]
        public void EventNameIsCaseInsensitive(string eventName) {
            EventEmitter.AddListener(eventName, EventCallback);

            IEventEmitterListener[] listeners = EventEmitter.Listeners(eventName.ToLower());

            Assert.Multiple(() => {
                // Only one
                Assert.That(EventEmitter.ListenersCount(eventName.ToLower()), Is.EqualTo(1));

                // Only one
                Assert.That(listeners, Has.Length.EqualTo(1));
            });
        }


        // Verify thrown exceptions
        #region Check thrown exceptions
        [TestCase("This_is_an_event_name_that_exceeds_128_characters_and_should_be_considered_invalid_1_2_3_4_5_6_7_8_9_0_or_gets_very_close_to_it_!")]
        public void AddListener_InvalidName_ThrowsInvalidNameException(string eventName) {
            try {
                EventEmitter.AddListener(eventName, EventCallback);
            } catch (Exception ex) {
                Assert.That(ex, Is.TypeOf<InvalidEventNameException>());
            }
        }

        [Test]
        public void AddListener_TooManyEvents_ThrowsTooManyEventsException() {
            try {
                for (int i = 0; i < EventEmitter.GetMaxEvents() + 1; i++)
                    EventEmitter.AddListener(Guid.NewGuid().ToString(), EventCallback);
            } catch (Exception ex) {
                Assert.That(ex, Is.TypeOf<TooManyEventsException>());
            }
        }

        [Test]
        public void AddListener_TooManyEventListeners_ThrowsTooManyEventListenersException() {
            try {
                for (int i = 0; i < EventEmitter.GetMaxListeners() + 1; i++)
                    EventEmitter.AddListener(TestEventName, EventCallback);
            } catch (Exception ex) {
                Assert.That(ex, Is.TypeOf<TooManyEventListenersException>());
            }
        }
        #endregion
    }
}
