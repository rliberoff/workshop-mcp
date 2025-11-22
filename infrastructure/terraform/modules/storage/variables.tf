variable "storage_account_name" {
  type = string
}

variable "resource_group_name" {
  type = string
}

variable "location" {
  type = string
}

variable "replication_type" {
  type    = string
  default = "LRS"
}

variable "cors_allowed_origins" {
  type    = list(string)
  default = ["*"]
}

variable "upload_sample_data" {
  type    = bool
  default = true
}

variable "sample_data_path" {
  type    = string
  default = "../../../../Data"
}

variable "tags" {
  type    = map(string)
  default = {}
}
