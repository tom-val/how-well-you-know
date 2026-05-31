# ACM certificate ARN (in us-east-1) for the frontend custom domain.
# Leave empty to serve on the default *.cloudfront.net URL.
variable "acm_certificate_arn" {
  type    = string
  default = ""
}

# Custom domains for the CloudFront distribution (only applied when a certificate is set).
variable "cloudfront_aliases" {
  type    = list(string)
  default = []
}

# Extra CORS origins the API should accept (e.g. http://localhost:5173 for local frontend dev).
variable "extra_cors_allowed_origins" {
  type    = list(string)
  default = []
}
