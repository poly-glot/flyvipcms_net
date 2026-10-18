import stylelint from 'stylelint';

const ruleName = 'flyvip/no-comments';

const messages = stylelint.utils.ruleMessages(ruleName, {
    rejected: 'Comments are not allowed; express intent through naming and structure.',
});

const rule = (enabled) => (root, result) => {
    if (!enabled) {
        return;
    }

    root.walkComments((comment) => {
        stylelint.utils.report({ message: messages.rejected, node: comment, result, ruleName });
    });
};

rule.ruleName = ruleName;
rule.messages = messages;

export default stylelint.createPlugin(ruleName, rule);
