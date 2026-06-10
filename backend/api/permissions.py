from rest_framework.permissions import BasePermission


class IsOrganizer(BasePermission):
    """Allow only users with is_organizer=True."""
    def has_permission(self, request, view):
        return request.user.is_authenticated and request.user.is_organizer


class IsEventOrganizer(BasePermission):
    """Allow only the organizer of the specific event."""
    def has_object_permission(self, request, view, obj):
        # obj can be Event or Application
        event = getattr(obj, 'event', obj)
        return event.organizer == request.user
