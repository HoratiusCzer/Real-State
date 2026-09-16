export function statusBadgeVariant(status: string): "default" | "primary" | "accent" | "success" | "destructive" {
  switch (status) {
    case "Approved":
      return "success";
    case "PendingReview":
      return "accent";
    case "Rejected":
      return "destructive";
    case "Draft":
    case "Archived":
    default:
      return "default";
  }
}

export function statusLabel(status: string): string {
  switch (status) {
    case "PendingReview":
      return "Pending Review";
    default:
      return status;
  }
}
