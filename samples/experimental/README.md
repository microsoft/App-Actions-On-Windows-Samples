<h1 align="center">
    Experimental App Actions on Windows 
</h1>

<h3 align="center">
  <a href="https://aka.ms/AppActionsOnWindows">Documentation</a>
  <span> · </span>
  <a href="https://aka.ms/AppActionsTestingPlayground">App Actions Testing Playground</a>
</h3>

This repo provides some examples of how to build and consume App Actions on Windows. Below are references to our documentation on learn.microsoft.com and a brief overview of what App Actions are. 

## What is an App Action?
An App Action is an atomic unit of functionality. Apps build and register actions, and then Windows or other apps can recommend registered actions to the user at contextually relevant times and locations within the user workflow. 

##  📋  Getting Started 
Actions can be implemented by handling URI launch activation, or by through COM activation by implementing the IActionProvider interface. For a walkthrough of implementing a simple app action provider using URI activation, see <a href="https://learn.microsoft.com/en-us/windows/ai/app-actions/actions-get-started?tabs=winget">Get started with App Actions</a>. There are also samples URI actions in this repo under thr uri folder. 

For information on implementing an app action using COM activation you can view the COM samples in the com folder of this repo. 

### Registration and Packaging 
Apps must have package identity in order to register an app action. The MSIX package manifest provides metadata about the actions that are supported by the provider app.

Actions are defined using a JSON format that provides metadata about one or more actions, which includes information like the unique identifier and description for the action as well as the list of inputs and outputs that the action operates on. The JSON action definition file is packaged with the provider app as content. The path to the file within the package is specified in the app package manifest so that the system can find and ingest the action definitions. For more information on on the JSON format for declaring actions, see <a href="https://learn.microsoft.com/en-us/windows/ai/app-actions/actions-json">Action definition JSON schema</a>.

### Entities 
An entity is an object that an App Action operates on. Actions take entities as inputs and can return entities as outputs. Entities are divided into subtypes to represent different types of content that an action can operate on, such Document, Photo, and Text. Each entity type has a set of properties that provide information related to each content type, such as the path or file extension of a file. Entities are expressed as JSON in the action definition JSON file to declare the inputs and outputs of an app action. A set of WinRT APIs representing entities are also available for working with entities in code. The entities referenced in this project are experimental and are subject to change.
