import { Box, Card, CardContent, Typography } from '@mui/material';
import {
  ResponsiveContainer,
  LineChart,
  Line,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
} from 'recharts';

const GRID_COLOR = '#e1e0d9';
const AXIS_COLOR = '#c3c2b7';
const MUTED_TEXT = '#898781';
const SEQUENTIAL_BLUE = '#2a78d6';
const CATEGORICAL_ORANGE = '#eb6834';

export const CHART_COLORS = [SEQUENTIAL_BLUE, CATEGORICAL_ORANGE];

export default function ChartCard({ title, type, data, series, height = 280, onBarClick }) {
  const seriesList = series ?? [{ dataKey: 'value', name: title, color: SEQUENTIAL_BLUE }];
  const rotateLabels = (data?.length ?? 0) > 6;

  return (
    <Card variant="outlined" sx={{ height: '100%' }}>
      <CardContent>
        <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 1 }}>{title}</Typography>
        {!data || data.length === 0 ? (
          <Box sx={{ height, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
            <Typography color="text.secondary" variant="body2">No data for this period.</Typography>
          </Box>
        ) : (
          <ResponsiveContainer width="100%" height={height}>
            {type === 'line' ? (
              <LineChart data={data} margin={{ top: 4, right: 8, left: 0, bottom: 4 }}>
                <CartesianGrid stroke={GRID_COLOR} vertical={false} />
                <XAxis dataKey="key" tick={{ fill: MUTED_TEXT, fontSize: 12 }} axisLine={{ stroke: AXIS_COLOR }} tickLine={false} />
                <YAxis tick={{ fill: MUTED_TEXT, fontSize: 12 }} axisLine={false} tickLine={false} width={48} />
                <Tooltip />
                {seriesList.length > 1 ? <Legend /> : null}
                {seriesList.map((s) => (
                  <Line
                    key={s.dataKey}
                    type="monotone"
                    dataKey={s.dataKey}
                    name={s.name}
                    stroke={s.color}
                    strokeWidth={2}
                    dot={{ r: 3 }}
                    activeDot={{ r: 5 }}
                  />
                ))}
              </LineChart>
            ) : (
              <BarChart
                data={data}
                margin={{ top: 4, right: 8, left: 0, bottom: rotateLabels ? 24 : 4 }}
                onClick={(state) => {
                  if (onBarClick && state?.activeLabel) onBarClick(state.activeLabel);
                }}
              >
                <CartesianGrid stroke={GRID_COLOR} vertical={false} />
                <XAxis
                  dataKey="key"
                  tick={{ fill: MUTED_TEXT, fontSize: 12 }}
                  axisLine={{ stroke: AXIS_COLOR }}
                  tickLine={false}
                  interval={0}
                  angle={rotateLabels ? -25 : 0}
                  textAnchor={rotateLabels ? 'end' : 'middle'}
                  height={rotateLabels ? 56 : 30}
                />
                <YAxis tick={{ fill: MUTED_TEXT, fontSize: 12 }} axisLine={false} tickLine={false} width={48} />
                <Tooltip cursor={{ fill: 'rgba(42,120,214,0.06)' }} />
                {seriesList.length > 1 ? <Legend /> : null}
                {seriesList.map((s) => (
                  <Bar
                    key={s.dataKey}
                    dataKey={s.dataKey}
                    name={s.name}
                    fill={s.color}
                    radius={[4, 4, 0, 0]}
                    cursor={onBarClick ? 'pointer' : 'default'}
                  />
                ))}
              </BarChart>
            )}
          </ResponsiveContainer>
        )}
      </CardContent>
    </Card>
  );
}
