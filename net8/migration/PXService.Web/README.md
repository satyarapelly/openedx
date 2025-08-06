# PXService.Web Docker

Build and run the PXService Web project in a container.

## Build

From the `net8/migration` directory:

```bash
docker build -f PXService.Web/Dockerfile -t pxservice-web .
```

## Run

```bash
docker run -p 8080:8080 pxservice-web
```

The service listens on port 8080 inside the container.
