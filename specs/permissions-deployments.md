# Provisioning Info for Permissions in Graph

## Abstract

The provisioning info file is a complementary JSON file to the JSON permission files that contains a list of permissions and the environments in which they are present. This document can be used to drive the permission publishing process as well as be used for discovery.

## Introduction

The JSON permissions model seeks to be the authoritative source of truth for for data on permissions to API mapping. We have restricted the contents of the JSON permission files to modeling data that has to be reviewed for every change. This does not include data about the permission in the cloud environments where it is intended to be published to. Some of that data is only known after the permission has been published to the environment and would have to either be automatically or manually updated back to the permissions model file after publishing e.g., the GUID of the permission in cloud environment. This should not require a review of the change to the permission file. The provisioning info file is complementary to the JSON permissions model files and allows us to keep the permissions model file as the authoritative source of truth for permissions modeling data while allowing us to maintain a separate source of truth for cloud availability data.

The provisioning info file is a single file for all permissions. Permissions are still the primary concept with a many to many relationship between permissions and cloud environments. Permission claim values are the top level members to enable quick lookup for provisioning for a given permission. Not all implemented permission schemes are published across all environments. The same permission would have different GUIDs in different environments and even when all schemes of a permission are published to the same environment, they would happen to have different unique identifiers. The provisioning info file allows us to model the cloud availability of a permission across different clouds for each of its schemes.

## Notational Conventions

The key words "MUST", "MUST NOT", "REQUIRED", "SHALL", "SHALL NOT", "SHOULD", "SHOULD NOT", "RECOMMENDED", "NOT RECOMMENDED", "MAY", and "OPTIONAL" in this document are to be interpreted as described in BCP 14 [RFC2119] [RFC8174] when, and only when, they appear in all capitals, as shown here.

## <a name="permissionsDeploymentDocumentObject"></a> PermissionsDeployments Document Object

This is the top level object in the provisioning info file. It contains a "permissionDeployments" property that contains a JSON object.

```json
{
    "permissionDeployments": {
    "Foo.Read.All": {
        "resourceAppId": "00000003-0000-0000-c000-000000000000",
        "environments": {
            "global": {
                "versions": {
                    "v1": {
                        "schemes": [
                        {
                            "DelegatedWork": {
                            "isHidden": false,
                            "isDisabled": false,
                            "id": "bed71653-0fa8-42a0-b33c-a1aab02ecda6"
                            }
                        }
                        ]
                    },
                    "beta": {
                        "schemes": [
                        {
                            "DelegatedWork": {
                            "isHidden": false,
                            "isDisabled": false,
                            "id": "bed71753-0fq8-42a2-b33c-c1acb02ecd17"
                            }
                        }
                        ]
                    }
                }
            }
        }
    }
    }
}
```

In this example, the permission "Foo.Read.All" is present in the global environment in both v1 and beta versions. The "DelegatedWork" scheme is present in both versions. Note that the same scheme of the same permission have permission has different GUIDs in the two clouds.

### permissionDeployments Object

The "permissionDeployments" object contains a map of  permission names to "permissionDeployment" objects. 

### <a name="permissionDeploymentObject"></a>PermissionDeployment Object

This is a JSON object that describes where a permission has been deployed and is available.

#### environments

The "environments" member is a JSON object that identifies the deployment environments in which the permission SHOULD be supported. Each key of the object is an environment identifier used for the [Environment Object](#environmentObject)

### <a name="environmentObject"></a>Environment Object

The environment object represents where a permission has been deployed.

#### versions
This member is a JSON object whose members are version objects. The key of each member is the version identifier used for the [Version Object](#versionObject).

### <a name="versionObject"></a>Version Object

The version object represents which version a permission is available in.

#### schemes
This  member is a JSON object whose members are scheme objects. The key of each member is the scheme identifier used for the [Scheme Object](#schemeObject).


### <a name="schemeObject"></a>Scheme Object
This member is a JSON object whose keys are permission scheme identifiers. The members of the scheme object are the provisioning info for the scheme in the cloud environment.

#### isHidden
The "isHidden" member is a boolean value that indicates if a permission should be publicly usable in the API.

#### isDisabled
The "isDisabled" member is a boolean value that indicates if a permission is disabled in the environment.

#### id
The "id" member is the GUID of the permission in the environment.

#### resourceAppId
The "resourceAppId" member value provides an identifier of the resource server that is used to enforce Conditional Access checks for this permission.
