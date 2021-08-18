
Some notes to improve / enable full use:

* Split out into an actual project!
* Are the send topics for Azure Devices (cloud and telemetry) fixed? (AWS is not), the lib should be able to override the default ones, and even send to custom ones! 
* What is the best way to add (and receive from) custom subscribe topics from the calling program when using the lib!
* Better documentation about ensuring "persistent" connections (or not) with documentation (including cloud policy doc for support)
* Best topic for Last Will and Testiment (is it fixed)?
* Add some unit tests (including ability to script auto provision cloud broker) or manual setup documents to ensure continued support.

AWS Specific:
* Greengrass?!
* Websocket support?!
