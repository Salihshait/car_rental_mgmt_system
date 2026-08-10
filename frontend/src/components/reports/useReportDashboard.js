import { useCallback, useEffect, useState } from 'react';

export function useReportDashboard(fetcher, initialFilters = {}) {
  const [filters, setFilters] = useState(initialFilters);
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const load = useCallback(
    (nextFilters) => {
      setLoading(true);
      setError('');
      fetcher(nextFilters)
        .then((result) => setData(result))
        .catch((err) => setError(err.message))
        .finally(() => setLoading(false));
    },
    [fetcher],
  );

  useEffect(() => {
    load(filters);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const applyFilters = (nextFilters) => {
    setFilters(nextFilters);
    load(nextFilters);
  };

  return { data, loading, error, filters, applyFilters };
}
