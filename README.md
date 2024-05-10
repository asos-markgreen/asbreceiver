# asbreceiver

Containerised app that reads messages from an ASB queue/subscription with a batch size of 500 and time out of 60 seconds.


Requires parameters (appsettings or environment variables):
- ServiceBus.ConnectionString: Connection for the service bus to read messages from
- ServiceBus.TopicName: topic name on the asb
- ServiceBus.SubscriptionName: Subscription name for the topic

Optional parameter:
- APPINSIGHTS_INSTRUMENTATIONKEY
- ServiceBus.SdkVersion: set to either "old" (Microsoft.Azure.ServiceBus) or "new" (Azure.Messaging.ServiceBus)
