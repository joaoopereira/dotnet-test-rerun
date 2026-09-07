module.exports = {
  extends: ['@commitlint/config-conventional'],
  ignores: [
    (message) => message.trim() === 'Initial plan',
  ],
};
