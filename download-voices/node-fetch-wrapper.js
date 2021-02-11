/*
* This is a custom made wrapper made to use `node-fetch`,
* but with usage as similar as possible to the deprecated
* `request-promise`.
* */

import _ from 'lodash'
import nodeFetch from 'node-fetch'

const RESPONSE_CONTENT_TYPES_BINARY = [
    `application/zip`,
]

const getRequestObj = (url, options) => {
    if (_.isEmpty(options)) {
        return url.toString()
    } else {
        const body = options.body
        if (body && _.isObject(body) && `getAll` in body) {
            options.body = body
        } else if (body && _.isObject(body)) {
            options.body = JSON.stringify(body)
        }
        return {
            url: url.toString(),
            ...options,
        }
    }
}

const simplyfySingleArrayValuesAsString = headers => {
    const simplifiedHeaders = _.clone(headers)
    _.forEach(
        _.keys(simplifiedHeaders),
        headerKey => {
            const headerValue = simplifiedHeaders[headerKey]
            if (_.isArray(headerValue) && headerValue.length === 1) {
                _.set(simplifiedHeaders, headerKey, _.head(headerValue))
            }
        }
    )
    return simplifiedHeaders
}

const isResponseBinaryFile = headers => {
    return RESPONSE_CONTENT_TYPES_BINARY.indexOf(_.get(headers, `content-type`))  !== -1
}

const get = async urlOrOptions => {
    let url
    let options
    let method = `GET`
    let responseIsJson = false
    let responseIsBinary = false
    if (_.isString(urlOrOptions)) {
        url = new URL(urlOrOptions)
    } else {
        url = new URL(urlOrOptions.url || urlOrOptions.uri)
        if (urlOrOptions.qs) {
            const existingSearch = url.search || ``
            url.search = (existingSearch ? existingSearch + `&` : ``) + _.join(
                _.map(
                    _.keys(urlOrOptions.qs),
                    qsKey => `${qsKey}=${urlOrOptions.qs[qsKey]}`
                ),
                `&`
            )
        }
        responseIsJson = urlOrOptions.json
        responseIsBinary = urlOrOptions.isBinary
        method = urlOrOptions.method || `GET`
        options = _.omit(_.cloneDeep(urlOrOptions), [`url`, `uri`, `qs`, `json`])
    }

    let response
    const requestObj = getRequestObj(url, options)
    const responseObj = await nodeFetch(requestObj.url || requestObj, _.omit(requestObj, `url`))

    const responseHeaders = simplyfySingleArrayValuesAsString(responseObj.headers.raw())

    // For HEAD, return response headers
    if (method === `HEAD`) {
        return responseHeaders
    }

    // For binary files, return a buffer
    if (responseIsBinary || isResponseBinaryFile(responseHeaders)) {
        return await responseObj.buffer()
    }

    // For all other methods, return body
    const textResponse = await responseObj.text()
    if (responseIsJson) {
        try {
            response = JSON.parse(textResponse)
        } catch (e) {
            response = textResponse
        }
    } else {
        response = textResponse
    }

    return response
}

export default get
