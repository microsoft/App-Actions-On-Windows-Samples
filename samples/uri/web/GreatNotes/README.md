# Retrieving File entities in a web app

For a web app to receive entities it must be registered as a [share target](https://developer.mozilla.org/en-US/docs/Web/Progressive_web_apps/Manifest/Reference/share_target) by adding a share_target entry in their webmanifest file.

File entities are retrieved in the `windowsActionFiles` field of the request's formData. The file entity's full path is not shared and instead only the file name is accessible.

More data regarding entities can be found in the event's data field which contains a json where each entry is a key value pair where the key is the name of the entity as defined in the actions manifest and the value contains the set of properties for the entity. 