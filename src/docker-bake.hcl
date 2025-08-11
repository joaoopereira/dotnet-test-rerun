variable "IMAGE" {default = "joaoopereira/dotnet-test-rerun"}
variable "VERSION" {default = "0.0.0"}
variable "DOTNET_VERSIONS" {default = "8.0 9.0"}

group "default" {
  targets = ["8", "9"]
}

target "build" {
  target = "build"
  args = {
    DOTNET_VERSIONS="${DOTNET_VERSIONS}"
  }
  labels = {
    "maintainer" = "mail@joaoopereira.com"
  }
}

target "8" {
  target = "runtime"
  tags = [
    "${IMAGE}:${VERSION}-net8",
    "${IMAGE}:${VERSION}-dotnet8"
    ]
  args = {
    DOTNET_VERSION="8.0"
  }
}

target "9" {
  target = "runtime"
  tags = [
    "${IMAGE}:${VERSION}",
    "${IMAGE}:${VERSION}-net9",
    "${IMAGE}:${VERSION}-dotnet9"
    ]
  args = {
    DOTNET_VERSION="9.0"
  }
}
