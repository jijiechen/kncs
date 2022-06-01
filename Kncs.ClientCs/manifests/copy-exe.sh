#!/bin/bash

# publish project:
# dotnet publish -r linux-x64 -p:PublishSingleFile=true

# Copy to test-pod:
kubectl cp ~/Projects/kncs/Kncs.ClientCs/bin/Debug/net7.0/linux-x64/publish/Kncs.ClientCs default/test-pod:/tmp/  -c dotnet-helper
kubectl exec -it test-pod -c dotnet-helper -- /bin/bash

