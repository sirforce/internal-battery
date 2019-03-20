module.exports = {
    entry: './client/react/index.js',
    module: {
        rules: [
            {
                test: /\.(js|jsx)$/,
                exclude: /node_modules/,
                use: ['babel-loader']
            }
        ]
    },
    resolve: {
        extensions: ['*', '.js', '.jsx']
    },
    output: {
        path: __dirname + '/wwwroot/js',
        publicPath: '/',
        filename: 'bundle.js'
    }
}