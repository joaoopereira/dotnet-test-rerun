variable "IMAGE" {default = "joaoopereira/dotnet-test-rerun"}
variable "VERSION" {default = "0.0.0"}

group "default" {
  targets = ["8", "9"]
}

target "8" {
  tags = [
    "${IMAGE}:${VERSION}-net8",
    "${IMAGE}:${VERSION}-dotnet8"
  ]
  args = {
    TARGET_DOTNET_VERSION = "8.0"
  }
  labels = {
    "maintainer" = "mail@joaoopereira.com"
    "org.opencontainers.image.version" = "${VERSION}"
    "org.opencontainers.image.target" = "net8.0"
  }
}

target "9" {
  tags = [
    "${IMAGE}:${VERSION}",
    "${IMAGE}:${VERSION}-net9",
    "${IMAGE}:${VERSION}-dotnet9"
  ]
  args = {
    TARGET_DOTNET_VERSION = "9.0"
  }
  labels = {
    "maintainer" = "mail@joaoopereira.com"
    "org.opencontainers.image.version" = "${VERSION}"
    "org.opencontainers.image.target" = "net9.0"
  }
}