# asbreceiver

Containerised app that reads messages from an ASB queue/subscription with a batch size of 500 and time out of 60 seconds.


Requires parameters (appsettings or environment variables):
- ServiceBus.ConnectionString: Connection for the service bus to read messages from
- ServiceBus.EntityName: EntityName for the subscription

Optional parameter:
- APPINSIGHTS_INSTRUMENTATIONKEY
