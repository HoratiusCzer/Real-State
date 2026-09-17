import type { Metadata } from "next";
import Link from "next/link";
import { Bell } from "lucide-react";
import { requireSession } from "@/lib/auth/session";
import { notificationsApi } from "@/lib/portal/api";
import { markAllNotificationsReadAction, markNotificationReadAction } from "@/lib/portal/actions";
import { Card } from "@/components/ui/card";
import { EmptyState } from "@/components/ui/empty-state";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

export const metadata: Metadata = { title: "Notifications" };

export default async function NotificationsPage() {
  const { accessToken } = await requireSession();
  const result = await notificationsApi.list(accessToken);
  const notifications = result.ok ? result.data : [];
  const hasUnread = notifications.some((n) => !n.isRead);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="font-heading text-2xl font-bold text-foreground">Notifications</h1>
        {hasUnread ? (
          <form action={markAllNotificationsReadAction}>
            <Button type="submit" variant="secondary" size="sm">
              Mark all as read
            </Button>
          </form>
        ) : null}
      </div>

      {!result.ok ? (
        <p className="text-sm text-destructive">Couldn&apos;t load notifications: {result.error}</p>
      ) : notifications.length === 0 ? (
        <EmptyState icon={Bell} title="No notifications yet" description="You're all caught up." />
      ) : (
        <ul className="space-y-2">
          {notifications.map((n) => (
            <li key={n.id}>
              <Card className={cn("flex items-start justify-between gap-4 p-4", !n.isRead && "border-accent")}>
                <div>
                  {n.linkUrl ? (
                    <Link
                      href={n.linkUrl}
                      className={cn("text-sm hover:underline", n.isRead ? "text-foreground" : "font-semibold text-foreground")}
                    >
                      {n.title}
                    </Link>
                  ) : (
                    <p className={cn("text-sm", n.isRead ? "text-foreground" : "font-semibold text-foreground")}>
                      {n.title}
                    </p>
                  )}
                  {n.body ? <p className="mt-1 text-sm text-muted-foreground">{n.body}</p> : null}
                  <p className="mt-1 text-xs text-muted-foreground">{new Date(n.createdAt).toLocaleString()}</p>
                </div>
                {!n.isRead ? (
                  <form action={markNotificationReadAction.bind(null, n.id)}>
                    <Button type="submit" variant="ghost" size="sm">
                      Mark read
                    </Button>
                  </form>
                ) : null}
              </Card>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
