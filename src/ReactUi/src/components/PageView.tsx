import { Box } from '@mui/material';
import React from 'react';

const PageView: React.FC<React.PropsWithChildren> = ({ children }) => {
  return <Box sx={{ padding: '2em' }}>{children}</Box>;
};

export default PageView;
