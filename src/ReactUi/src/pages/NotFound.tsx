import React from 'react';
import PageView from '../components/PageView';
import { Button } from '@mui/material';

const messages = [
  'The page you are looking for is in another castle.',
  'This is not the page you are looking for.',
  'How did you get here?',
  'Nothing to see here.',
  'Page not found.',
  'Move along, nothing to see here.',
  'Not all those who wander are lost.',
  "I'm running out of messages to put here.",
  'If you got here by using the site normally, please raise an issue on GitHub.',
  'Keep looking, you will find it eventually.',
];

const NotFound = () => {
  React.useEffect(() => {
    document.title = 'Not Found';
  }, []);

  return (
    <PageView>
      <h1>404</h1>
      <p style={{ fontSize: 24 }}>
        {messages[Math.floor(Math.random() * messages.length)]}
      </p>
      <Button onClick={() => window.history.back()}>Go Back</Button>
    </PageView>
  );
};

export default NotFound;
