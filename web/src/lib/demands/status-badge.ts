export function demandStatusBadgeVariant(status: string): "default" | "primary" | "accent" | "success" | "destructive" {
  switch (status) {
    case "Active":
      return "success";
    case "Fulfilled":
      return "primary";
    case "Draft":
    case "Archived":
    default:
      return "default";
  }
}
