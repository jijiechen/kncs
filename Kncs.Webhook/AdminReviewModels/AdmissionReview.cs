//----------------------
// <auto-generated>
//     Generated using the NSwag toolchain v13.15.10.0 (NJsonSchema v10.6.10.0 (Newtonsoft.Json v13.0.0.0)) (http://NSwag.org)
// </auto-generated>
//----------------------

#pragma warning disable 108 // Disable "CS0108 '{derivedDto}.ToJson()' hides inherited member '{dtoBase}.ToJson()'. Use the new keyword if hiding was intended."
#pragma warning disable 114 // Disable "CS0114 '{derivedDto}.RaisePropertyChanged(String)' hides inherited member 'dtoBase.RaisePropertyChanged(String)'. To make the current member override that implementation, add the override keyword. Otherwise add the new keyword."
#pragma warning disable 472 // Disable "CS0472 The result of the expression is always 'false' since a value of type 'Int32' is never equal to 'null' of type 'Int32?'
#pragma warning disable 1573 // Disable "CS1573 Parameter '...' has no matching param tag in the XML comment for ...
#pragma warning disable 1591 // Disable "CS1591 Missing XML comment for publicly visible type or member ..."
#pragma warning disable 8073 // Disable "CS8073 The result of the expression is always 'false' since a value of type 'T' is never equal to 'null' of type 'T?'"
#pragma warning disable 3016 // Disable "CS3016 Arrays as attribute arguments is not CLS-compliant"
#pragma warning disable 8603 // Disable "CS8603 Possible null reference return"

namespace Kncs.Webhook
{
    using System = global::System;

    

    /// <summary>
    /// RawExtension is used to hold extensions in external versions.
    /// <br/>
    /// <br/>To use this, make a field which has RawExtension as its type in your external, versioned struct, and Object in your internal struct. You also need to register your various plugin types.
    /// <br/>
    /// <br/>// Internal package: type MyAPIObject struct {
    /// <br/>	runtime.TypeMeta `json:",inline"`
    /// <br/>	MyPlugin runtime.Object `json:"myPlugin"`
    /// <br/>} type PluginA struct {
    /// <br/>	AOption string `json:"aOption"`
    /// <br/>}
    /// <br/>
    /// <br/>// External package: type MyAPIObject struct {
    /// <br/>	runtime.TypeMeta `json:",inline"`
    /// <br/>	MyPlugin runtime.RawExtension `json:"myPlugin"`
    /// <br/>} type PluginA struct {
    /// <br/>	AOption string `json:"aOption"`
    /// <br/>}
    /// <br/>
    /// <br/>// On the wire, the JSON will look something like this: {
    /// <br/>	"kind":"MyAPIObject",
    /// <br/>	"apiVersion":"v1",
    /// <br/>	"myPlugin": {
    /// <br/>		"kind":"PluginA",
    /// <br/>		"aOption":"foo",
    /// <br/>	},
    /// <br/>}
    /// <br/>
    /// <br/>So what happens? Decode first uses json or yaml to unmarshal the serialized data into your external MyAPIObject. That causes the raw JSON to be stored, but not unpacked. The next step is to copy (using pkg/conversion) into the internal struct. The runtime package's DefaultScheme has conversion functions installed which will unpack the JSON stored in RawExtension, turning it into the correct object type, and storing it in the Object. (TODO: In the case where the object is of an unknown type, a runtime.Unknown object will be created and stored.)
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.10.0 (NJsonSchema v10.6.10.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class RawExtension
    {

    }

    /// <summary>
    /// AdmissionRequest describes the admission.Attributes for the admission request.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.10.0 (NJsonSchema v10.6.10.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class AdmissionRequest
    {
        /// <summary>
        /// DryRun indicates that modifications will definitely not be persisted for this request. Defaults to false.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("dryRun")]
        public bool? DryRun { get; set; }

        /// <summary>
        /// Kind is the fully-qualified type of object being submitted (for example, v1.Pod or autoscaling.v1.Scale)
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("kind")]
        [System.ComponentModel.DataAnnotations.Required]
        public GroupVersionKind Kind { get; set; } = new GroupVersionKind();

        /// <summary>
        /// Name is the name of the object as presented in the request.  On a CREATE operation, the client may omit name and rely on the server to generate the name.  If that is the case, this field will contain an empty string.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Namespace is the namespace associated with the request (if any).
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("namespace")]
        public string Namespace { get; set; }

        /// <summary>
        /// Object is the object from the incoming request.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("object")]
        public System.Text.Json.JsonElement Object { get; set; }

        /// <summary>
        /// OldObject is the existing object. Only populated for DELETE and UPDATE requests.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("oldObject")]
        public System.Text.Json.JsonElement OldObject { get; set; }

        /// <summary>
        /// Operation is the operation being performed. This may be different than the operation requested. e.g. a patch can result in either a CREATE or UPDATE Operation.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("operation")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Operation { get; set; }

        /// <summary>
        /// Options is the operation option structure of the operation being performed. e.g. `meta.k8s.io/v1.DeleteOptions` or `meta.k8s.io/v1.CreateOptions`. This may be different than the options the caller provided. e.g. for a patch request the performed Operation might be a CREATE, in which case the Options will a `meta.k8s.io/v1.CreateOptions` even though the caller provided `meta.k8s.io/v1.PatchOptions`.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("options")]
        public System.Text.Json.JsonElement Options { get; set; }

        /// <summary>
        /// RequestKind is the fully-qualified type of the original API request (for example, v1.Pod or autoscaling.v1.Scale). If this is specified and differs from the value in "kind", an equivalent match and conversion was performed.
        /// <br/>
        /// <br/>For example, if deployments can be modified via apps/v1 and apps/v1beta1, and a webhook registered a rule of `apiGroups:["apps"], apiVersions:["v1"], resources: ["deployments"]` and `matchPolicy: Equivalent`, an API request to apps/v1beta1 deployments would be converted and sent to the webhook with `kind: {group:"apps", version:"v1", kind:"Deployment"}` (matching the rule the webhook registered for), and `requestKind: {group:"apps", version:"v1beta1", kind:"Deployment"}` (indicating the kind of the original API request).
        /// <br/>
        /// <br/>See documentation for the "matchPolicy" field in the webhook configuration type for more details.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("requestKind")]
        public GroupVersionKind RequestKind { get; set; }

        /// <summary>
        /// RequestResource is the fully-qualified resource of the original API request (for example, v1.pods). If this is specified and differs from the value in "resource", an equivalent match and conversion was performed.
        /// <br/>
        /// <br/>For example, if deployments can be modified via apps/v1 and apps/v1beta1, and a webhook registered a rule of `apiGroups:["apps"], apiVersions:["v1"], resources: ["deployments"]` and `matchPolicy: Equivalent`, an API request to apps/v1beta1 deployments would be converted and sent to the webhook with `resource: {group:"apps", version:"v1", resource:"deployments"}` (matching the resource the webhook registered for), and `requestResource: {group:"apps", version:"v1beta1", resource:"deployments"}` (indicating the resource of the original API request).
        /// <br/>
        /// <br/>See documentation for the "matchPolicy" field in the webhook configuration type.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("requestResource")]
        public GroupVersionResource RequestResource { get; set; }

        /// <summary>
        /// RequestSubResource is the name of the subresource of the original API request, if any (for example, "status" or "scale") If this is specified and differs from the value in "subResource", an equivalent match and conversion was performed. See documentation for the "matchPolicy" field in the webhook configuration type.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("requestSubResource")]
        public string RequestSubResource { get; set; }

        /// <summary>
        /// Resource is the fully-qualified resource being requested (for example, v1.pods)
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("resource")]
        [System.ComponentModel.DataAnnotations.Required]
        public GroupVersionResource Resource { get; set; } = new GroupVersionResource();

        /// <summary>
        /// SubResource is the subresource being requested, if any (for example, "status" or "scale")
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("subResource")]
        public string SubResource { get; set; }

        /// <summary>
        /// UID is an identifier for the individual request/response. It allows us to distinguish instances of requests which are otherwise identical (parallel requests, requests when earlier requests did not modify etc) The UID is meant to track the round trip (request/response) between the KAS and the WebHook, not the user request. It is suitable for correlating log entries between the webhook and apiserver, for either auditing or debugging.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("uid")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Uid { get; set; }

        /// <summary>
        /// UserInfo is information about the requesting user
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("userInfo")]
        [System.ComponentModel.DataAnnotations.Required]
        public UserInfo UserInfo { get; set; } = new UserInfo();

    }

    /// <summary>
    /// AdmissionResponse describes an admission response.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.10.0 (NJsonSchema v10.6.10.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class AdmissionResponse
    {
        /// <summary>
        /// Allowed indicates whether or not the admission request was permitted.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("allowed")]
        public bool Allowed { get; set; }

        /// <summary>
        /// AuditAnnotations is an unstructured key value map set by remote admission controller (e.g. error=image-blacklisted). MutatingAdmissionWebhook and ValidatingAdmissionWebhook admission controller will prefix the keys with admission webhook name (e.g. imagepolicy.example.com/error=image-blacklisted). AuditAnnotations will be provided by the admission webhook to add additional context to the audit log for this request.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("auditAnnotations")]
        public System.Collections.Generic.IDictionary<string, string> AuditAnnotations { get; set; }

        /// <summary>
        /// The patch body. Currently we only support "JSONPatch" which implements RFC 6902.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("patch")]
        public byte[] Patch { get; set; }

        /// <summary>
        /// The type of Patch. Currently we only allow "JSONPatch".
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("patchType")]
        public string PatchType { get; set; }

        /// <summary>
        /// Result contains extra details into why an admission request was denied. This field IS NOT consulted in any way if "Allowed" is "true".
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public Status Status { get; set; }

        /// <summary>
        /// UID is an identifier for the individual request/response. This must be copied over from the corresponding AdmissionRequest.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("uid")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Uid { get; set; }

        /// <summary>
        /// warnings is a list of warning messages to return to the requesting API client. Warning messages describe a problem the client making the API request should correct or be aware of. Limit warnings to 120 characters if possible. Warnings over 256 characters and large numbers of warnings may be truncated.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("warnings")]
        public System.Collections.Generic.ICollection<string> Warnings { get; set; }

    }

    /// <summary>
    /// AdmissionReview describes an admission review request/response.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.10.0 (NJsonSchema v10.6.10.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class AdmissionReview
    {
        /// <summary>
        /// APIVersion defines the versioned schema of this representation of an object. Servers should convert recognized schemas to the latest internal value, and may reject unrecognized values. More info: https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#resources
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("apiVersion")]
        public string ApiVersion { get; set; }

        /// <summary>
        /// Kind is a string value representing the REST resource this object represents. Servers may infer this from the endpoint the client submits requests to. Cannot be updated. In CamelCase. More info: https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#types-kinds
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("kind")]
        public string Kind { get; set; }

        /// <summary>
        /// Request describes the attributes for the admission request.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("request")]
        public AdmissionRequest Request { get; set; }

        /// <summary>
        /// Response describes the attributes for the admission response.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("response")]
        public AdmissionResponse Response { get; set; }

    }

    /// <summary>
    /// GroupVersionKind unambiguously identifies a kind.  It doesn't anonymously include GroupVersion to avoid automatic coersion.  It doesn't use a GroupVersion to avoid custom marshalling
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.10.0 (NJsonSchema v10.6.10.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class GroupVersionKind
    {
        [System.Text.Json.Serialization.JsonPropertyName("group")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Group { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("kind")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Kind { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("version")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Version { get; set; }

    }

    /// <summary>
    /// GroupVersionResource unambiguously identifies a resource.  It doesn't anonymously include GroupVersion to avoid automatic coersion.  It doesn't use a GroupVersion to avoid custom marshalling
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.10.0 (NJsonSchema v10.6.10.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class GroupVersionResource
    {
        [System.Text.Json.Serialization.JsonPropertyName("group")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Group { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("resource")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Resource { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("version")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Version { get; set; }

    }

    /// <summary>
    /// ListMeta describes metadata that synthetic resources must have, including lists and various status objects. A resource may have only one of {ObjectMeta, ListMeta}.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.10.0 (NJsonSchema v10.6.10.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class ListMeta
    {
        /// <summary>
        /// continue may be set if the user set a limit on the number of items returned, and indicates that the server has more data available. The value is opaque and may be used to issue another request to the endpoint that served this list to retrieve the next set of available objects. Continuing a consistent list may not be possible if the server configuration has changed or more than a few minutes have passed. The resourceVersion field returned when using this continue value will be identical to the value in the first response, unless you have received this token from an error message.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("continue")]
        public string Continue { get; set; }

        /// <summary>
        /// remainingItemCount is the number of subsequent items in the list which are not included in this list response. If the list request contained label or field selectors, then the number of remaining items is unknown and the field will be left unset and omitted during serialization. If the list is complete (either because it is not chunking or because this is the last chunk), then there are no more remaining items and this field will be left unset and omitted during serialization. Servers older than v1.15 do not set this field. The intended use of the remainingItemCount is *estimating* the size of a collection. Clients should not rely on the remainingItemCount to be set or to be exact.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("remainingItemCount")]
        public long? RemainingItemCount { get; set; }

        /// <summary>
        /// String that identifies the server's internal version of this object that can be used by clients to determine when objects have changed. Value must be treated as opaque by clients and passed unmodified back to the server. Populated by the system. Read-only. More info: https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#concurrency-control-and-consistency
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("resourceVersion")]
        public string ResourceVersion { get; set; }

        /// <summary>
        /// selfLink is a URL representing this object. Populated by the system. Read-only.
        /// <br/>
        /// <br/>DEPRECATED Kubernetes will stop propagating this field in 1.20 release and the field is planned to be removed in 1.21 release.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("selfLink")]
        public string SelfLink { get; set; }

    }

    /// <summary>
    /// Status is a return value for calls that don't return other objects.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.10.0 (NJsonSchema v10.6.10.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class Status
    {
        /// <summary>
        /// APIVersion defines the versioned schema of this representation of an object. Servers should convert recognized schemas to the latest internal value, and may reject unrecognized values. More info: https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#resources
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("apiVersion")]
        public string ApiVersion { get; set; }

        /// <summary>
        /// Suggested HTTP return code for this status, 0 if not set.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("code")]
        public int? Code { get; set; }

        /// <summary>
        /// Extended data associated with the reason.  Each reason may define its own extended details. This field is optional and the data returned is not guaranteed to conform to any schema except that defined by the reason type.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("details")]
        public StatusDetails Details { get; set; }

        /// <summary>
        /// Kind is a string value representing the REST resource this object represents. Servers may infer this from the endpoint the client submits requests to. Cannot be updated. In CamelCase. More info: https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#types-kinds
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("kind")]
        public string Kind { get; set; }

        /// <summary>
        /// A human-readable description of the status of this operation.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("message")]
        public string Message { get; set; }

        /// <summary>
        /// Standard list metadata. More info: https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#types-kinds
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("metadata")]
        public ListMeta Metadata { get; set; }

        /// <summary>
        /// A machine-readable description of why this operation is in the "Failure" status. If this value is empty there is no information available. A Reason clarifies an HTTP status code but does not override it.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("reason")]
        public string Reason { get; set; }

        /// <summary>
        /// Status of the operation. One of: "Success" or "Failure". More info: https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#spec-and-status
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public string Status1 { get; set; }

    }

    /// <summary>
    /// StatusCause provides more information about an api.Status failure, including cases when multiple errors are encountered.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.10.0 (NJsonSchema v10.6.10.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class StatusCause
    {
        /// <summary>
        /// The field of the resource that has caused this error, as named by its JSON serialization. May include dot and postfix notation for nested attributes. Arrays are zero-indexed.  Fields may appear more than once in an array of causes due to fields having multiple errors. Optional.
        /// <br/>
        /// <br/>Examples:
        /// <br/>  "name" - the field "name" on the current resource
        /// <br/>  "items[0].name" - the field "name" on the first array entry in "items"
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("field")]
        public string Field { get; set; }

        /// <summary>
        /// A human-readable description of the cause of the error.  This field may be presented as-is to a reader.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("message")]
        public string Message { get; set; }

        /// <summary>
        /// A machine-readable description of the cause of the error. If this value is empty there is no information available.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("reason")]
        public string Reason { get; set; }

    }

    /// <summary>
    /// StatusDetails is a set of additional properties that MAY be set by the server to provide additional information about a response. The Reason field of a Status object defines what attributes will be set. Clients must ignore fields that do not match the defined type of each attribute, and should assume that any attribute may be empty, invalid, or under defined.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.10.0 (NJsonSchema v10.6.10.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class StatusDetails
    {
        /// <summary>
        /// The Causes array includes more details associated with the StatusReason failure. Not all StatusReasons may provide detailed causes.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("causes")]
        public System.Collections.Generic.ICollection<StatusCause> Causes { get; set; }

        /// <summary>
        /// The group attribute of the resource associated with the status StatusReason.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("group")]
        public string Group { get; set; }

        /// <summary>
        /// The kind attribute of the resource associated with the status StatusReason. On some operations may differ from the requested resource Kind. More info: https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#types-kinds
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("kind")]
        public string Kind { get; set; }

        /// <summary>
        /// The name attribute of the resource associated with the status StatusReason (when there is a single name which can be described).
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// If specified, the time in seconds before the operation should be retried. Some errors may indicate the client must take an alternate action - for those errors this field may indicate how long to wait before taking the alternate action.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("retryAfterSeconds")]
        public int? RetryAfterSeconds { get; set; }

        /// <summary>
        /// UID of the resource. (when there is a single resource which can be described). More info: http://kubernetes.io/docs/user-guide/identifiers#uids
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("uid")]
        public string Uid { get; set; }

    }

    /// <summary>
    /// UserInfo holds the information about the user needed to implement the user.Info interface.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "13.15.10.0 (NJsonSchema v10.6.10.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class UserInfo
    {
        /// <summary>
        /// Any additional information provided by the authenticator.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("extra")]
        public System.Collections.Generic.IDictionary<string, System.Collections.Generic.ICollection<string>> Extra { get; set; }

        /// <summary>
        /// The names of groups this user is a part of.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("groups")]
        public System.Collections.Generic.ICollection<string> Groups { get; set; }

        /// <summary>
        /// A unique value that identifies this user across time. If this user is deleted and another user by the same name is added, they will have different UIDs.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("uid")]
        public string Uid { get; set; }

        /// <summary>
        /// The name that uniquely identifies this user among all active users.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("username")]
        public string Username { get; set; }

    }



}

#pragma warning restore 1591
#pragma warning restore 1573
#pragma warning restore  472
#pragma warning restore  114
#pragma warning restore  108
#pragma warning restore 3016
#pragma warning restore 8603