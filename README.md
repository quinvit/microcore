# Introduction

A forked version of https://github.com/gigya/microdot framework to .net core 2.2 with some changes:

1. Replace NLog by https://serilog.net/
2. Remove Orleans dependencies
3. Remove Window Service feature to fully compatible in linux/container environment
4. Replace Metrics.NET by https://www.app-metrics.io/


Original features:

1. A service container which accepts command-line parameters that define how your service runs, e.g. as a command-line process, with or without console logs, the port your service will use to listen to incoming requests, whether it runs alone or as part of a cluster (and the cluster name to join), and whether it should shut down gracefully once a monitored parent PID exits. Sensible defaults are used based on your build configuration (Release/Debug).
inter-service RPC allowing services to call one another. Each service exposes one or more C# interfaces, and clients call it by receiving an instance of an interface that performs transparent RPC using JSON over HTTP. This includes client-side load balancing (no need for a load balancer in front of your service), failover support, and secure comunication via HTTPS with certificates validations for sensitive services, if needed.
2. Client-side, opt-in, transparent response caching between services. Useful to reduce end-to-end latency when many of your services rely on a few core services that serve relatively static data that is allowed to be eventually consistent. Also useful to reduce the impact of said services failing, while their responses are still cached by clients.
3. Logging and Distributed Tracing facilities to help diagnosing issues in production, such as Exception Tracking. Client- and server-side events are emitted for every call and can be used to trace how a request was handled across all services (the call tree), and the latency each one contributed.
4. Client-side Service discovery that supports manual configuration-based discovery (Consul stuffs are removed).
5. All components emit performance metrics via https://www.app-metrics.io/ for real-time performance monitoring.
6. Detailed Health Checks are provided for each subsystem, and can easily be extended to cover your service's external dependencies.
7. A hierarchical configuration system based on XML files which allows overriding values based on where and how the microservice is hosted. The configuration is consumed from code via strongly-typed objects with automatic mapping and is refreshed at real time when XML files change.
8. Highly modular design and first-class dependency injection support using Ninject, allowing you to swap out every component with your own implementation if needed.

# Sample

[Angular with MSA on Azure](https://github.com/quinvit/microcore/tree/master/Sample/Angular)
