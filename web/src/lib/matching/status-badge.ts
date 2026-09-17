export function matchStatusBadgeVariant(status: string): "default" | "primary" | "accent" | "success" | "destructive" {
  switch (status) {
    case "New":
      return "accent";
    case "Shortlisted":
      return "success";
    case "Dismissed":
      return "destructive";
    case "Reopened":
      return "primary";
    default:
      return "default";
  }
}
