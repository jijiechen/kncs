#!/bin/bash

# cmd_nswag=$(command -v nswag || true)
# if [ "$cmd_nswag" == "" ]; then
#     npm install -g nswag
#     nswag version /runtime:NetCore31
# fi

# fetch swagger scheme from https://gist.github.com/bergeron/70ca86cf31762e16f18b2be3c549a074
if [ ! -f "./admission.swagger.json" ]; then
  curl --fail -o admission.swagger.json https://gist.githubusercontent.com/bergeron/70ca86cf31762e16f18b2be3c549a074/raw/77c67214eff1c9edf7b133947c0d0ff557dcdc6f/k8s.io.api.admission.v1.swagger.json
fi

nswag openapi2csclient /input:admission.swagger.json  /classname:AdmissionReview /namespace:Kncs.Webhook /output:AdmissionReview.cs
sed -i 's/public RawExtension/public System.Text.Json.JsonElement/g' ./AdmissionReview.cs
sed -i 's/\[Newtonsoft.Json.JsonProperty/[System.Text.Json.Serialization.JsonPropertyName/g' ./AdmissionReview.cs
sed -i 's/,\ Required.*/)]/g' ./AdmissionReview.cs
