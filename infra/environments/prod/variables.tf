# ACM certificate ARN (in us-east-1) for the frontend custom domain.
# Wildcard *.valiunas.dev cert, so it covers quiz.valiunas.dev.
# Set to "" to serve on the default *.cloudfront.net URL instead.
variable "acm_certificate_arn" {
  type    = string
  default = "arn:aws:acm:us-east-1:054630617930:certificate/b65a278b-6ba4-4ed0-b5f4-87e0dd9e9210"
}

# Custom domains for the CloudFront distribution (only applied when a certificate is set).
variable "cloudfront_aliases" {
  type    = list(string)
  default = ["quiz.valiunas.dev"]
}

# Extra CORS origins the API should accept (e.g. http://localhost:5173 for local frontend dev).
variable "extra_cors_allowed_origins" {
  type    = list(string)
  default = []
}

# OpenAI API key for AI question suggestions. Empty disables AI (clients fall back to the
# static question bank). Set via the OPENAI_API_KEY GitHub secret.
variable "openai_api_key" {
  type      = string
  sensitive = true
  default   = ""
}
