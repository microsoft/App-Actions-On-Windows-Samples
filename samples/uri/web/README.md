# Progressive Web Apps

Actions can be backed by Progressive Web Apps. Progressive Web Apps require a host app to run, in most cases, the host app is Microsoft Edge.

Currently, Edge Stable only suppports action invocation of actions that can include all necessary information in an Uri (i.e. web+myapp://playMovie?title=Cool%20Movie). 
This behavior can be observed in the NotFlix sample in this directory.

Edge Canary supports Text and File entity types. To enable the latest supported features in Edge Canary please visit `edge://flags` and enable the flag 
"Enable App Actions on Windows for web apps". The GreatNotes sample in this directory shows how can a developer make use of entities in their web app.