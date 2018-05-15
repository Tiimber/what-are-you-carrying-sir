﻿using System.Collections.Generic;
using System.Linq;

public class PubSub {

	static Dictionary<string, List<IPubSub>> subscriptions = new Dictionary<string, List<IPubSub>> (); 
	static Dictionary<string, Dictionary<IPubSub, int>> subscriptionsWithPriorities = new Dictionary<string, Dictionary<IPubSub, int>> ();

	public static void subscribe (string message, IPubSub subscriber, int prio = 0) {
		if (!subscriptions.ContainsKey (message)) {
			subscriptions.Add (message, new List<IPubSub>());
			subscriptionsWithPriorities.Add (message, new Dictionary<IPubSub, int>());
		}
		List<IPubSub> messageEntry = subscriptions[message];
		if (!messageEntry.Contains (subscriber)) {
			messageEntry.Add (subscriber);
			subscriptionsWithPriorities[message].Add (subscriber, prio);
		}
	}

	public static void unsubscribe (string message, IPubSub subscriber) {
		if (subscriptions.ContainsKey (message)) {
			List<IPubSub> messageEntry = subscriptions[message];
			if (messageEntry.Contains (subscriber)) {
				messageEntry.Remove (subscriber);
				subscriptionsWithPriorities [message].Remove (subscriber);
				if (messageEntry.Count == 0) {
					subscriptions.Remove (message);
					subscriptionsWithPriorities.Remove (message);
				}
			}
		}
	}

	public static void unsubscribeAllForSubscriber (IPubSub subscriber) {
        List<KeyValuePair<string, IPubSub>> unsubscribes = new List<KeyValuePair<string, IPubSub>>();

		foreach (KeyValuePair<string, List<IPubSub>> messageEntries in subscriptions) {
			foreach (IPubSub subscribeObj in messageEntries.Value) {
				if (subscribeObj == subscriber) {
                    unsubscribes.Add(new KeyValuePair<string, IPubSub>(messageEntries.Key, subscriber));
					break;
				}
			}
		}

		foreach (KeyValuePair<string, IPubSub> doUnsubscribe in unsubscribes) {
            unsubscribe(doUnsubscribe.Key, doUnsubscribe.Value);
        }
	}

	public static void publish (string message, object data = null) {
		if (subscriptions.ContainsKey (message)) {
			Dictionary<IPubSub, int> subscriptionsUnsorted = subscriptionsWithPriorities [message];
			List<IPubSub> subscriptionsSorted = subscriptionsUnsorted.OrderByDescending (entry => entry.Value).Select (entry => entry.Key).ToList();

			int priorityMinLimit = int.MinValue;
			PROPAGATION propagation;
			foreach (IPubSub subscriber in subscriptionsSorted) {
				// Check if we should continue due to priority and propagation rules
				int priority = subscriptionsUnsorted[subscriber];
				if (priority < priorityMinLimit) {
					break;
				}

				propagation = subscriber.onMessage (message, data);
                switch (propagation) {
                    case PROPAGATION.STOP_IMMEDIATELY:
                        return;
                    break;
                    case PROPAGATION.STOP_AFTER_SAME_TYPE:
						priorityMinLimit = priority;
                    break;
					case PROPAGATION.CONTINUE_WITH_OTHER_TYPES:
						// TODO - Propagate more types?
					case PROPAGATION.DEFAULT:
                    default:
                    break;
                }
			}
		}
	}
}
