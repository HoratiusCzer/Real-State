export function contentStatusBadgeVariant(status: string): "default" | "primary" | "accent" | "success" | "destructive" {
  switch (status) {
    case "Draft":
      return "default";
    case "Review":
      return "accent";
    case "Published":
      return "success";
    case "Archived":
      return "destructive";
    default:
      return "default";
  }
}
