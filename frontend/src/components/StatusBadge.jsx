const STATUS_STYLES = {
  pending: 'bg-yellow-100 text-yellow-800 border-yellow-300',
  approved: 'bg-green-100 text-green-800 border-green-300',
  rejected: 'bg-red-100 text-red-800 border-red-300',
  cancelled: 'bg-gray-100 text-gray-600 border-gray-300',
};

const STATUS_LABELS = {
  pending: 'В обработке',
  approved: 'Подтверждено',
  rejected: 'Отклонено',
  cancelled: 'Отменено',
};

const ATTENDANCE_STYLES = {
  unknown: 'bg-gray-100 text-gray-600',
  attended: 'bg-green-100 text-green-800',
  missed: 'bg-red-100 text-red-800',
};

const ATTENDANCE_LABELS = {
  unknown: '—',
  attended: 'Посетил',
  missed: 'Не пришёл',
};

export function StatusBadge({ status }) {
  return (
    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border ${STATUS_STYLES[status] || ''}`}>
      <span className={`w-1.5 h-1.5 rounded-full mr-1.5 ${
        status === 'pending' ? 'bg-yellow-500' :
        status === 'approved' ? 'bg-green-500' :
        status === 'rejected' ? 'bg-red-500' : 'bg-gray-400'
      }`} />
      {STATUS_LABELS[status] || status}
    </span>
  );
}

export function AttendanceBadge({ attendance }) {
  if (attendance === 'unknown') return null;
  return (
    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${ATTENDANCE_STYLES[attendance] || ''}`}>
      {ATTENDANCE_LABELS[attendance] || attendance}
    </span>
  );
}
