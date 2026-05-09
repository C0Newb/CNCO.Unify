const importScriptShim = (code: string): string => {
    return URL.createObjectURL(new Blob([code], { type: 'text/javascript' }));
};
export default importScriptShim;