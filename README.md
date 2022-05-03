
使用 CSharp 开发 Kubernetes 原生基础设施
======


包含如下示例：

1. kncs.CmdExecuter 展示如何在 C# 中读取、生成并编辑 YAML，最终传入 kubectl 命令行，动态地在 Kubernetes 集群中创建 Pod
1. kncs.ClientCs 展示如何以 C# 编程的方式与 Kubernetes 集群交互，读取和监视 Pod 资源的状态
1. kncs.Webhook 展示如何用 C# 实现 Kubernetes Webhook，实现对 Pod 镜像的检查，并实现类似 Istio 中的容器自动注入的功能
1. kncs.CrdController 展示如何实现自定义资源类型 CRD：为 Kubernetes 集群安装新的 CSharpApp 资源类型

